using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace NetDoctor.App.Services;

/// <summary>
/// Minimal DNS client that queries a *specific* resolver over UDP/53 and reads back the
/// A records. The built-in <see cref="System.Net.Dns"/> only uses the system resolver, so
/// this is what lets the Iran edition compare the system DNS against public foreign DNS
/// (1.1.1.1, 8.8.8.8) and Iranian DNS (Shecan, Electro, ...) to locate the real fault.
/// </summary>
internal static class DnsProbe
{
    internal sealed record DnsResult(bool Success, int AddressCount, long ElapsedMs, string? FirstAddress, string Message);

    public static async Task<DnsResult> ResolveAsync(
        string serverIp,
        string host,
        int timeoutMs,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            using var udp = new UdpClient(AddressFamily.InterNetwork);
            udp.Connect(IPAddress.Parse(serverIp), 53);

            var query = BuildQuery(host);
            await udp.SendAsync(query, cancellationToken);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(timeoutMs);

            UdpReceiveResult response;
            try
            {
                response = await udp.ReceiveAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                stopwatch.Stop();
                return new DnsResult(false, 0, stopwatch.ElapsedMilliseconds, null, "timed out");
            }

            stopwatch.Stop();
            var (count, first, rcode) = ParseAnswers(response.Buffer);

            if (rcode != 0)
            {
                return new DnsResult(false, 0, stopwatch.ElapsedMilliseconds, null, $"server returned code {rcode}");
            }

            if (count == 0)
            {
                return new DnsResult(false, 0, stopwatch.ElapsedMilliseconds, null, "no address records");
            }

            return new DnsResult(true, count, stopwatch.ElapsedMilliseconds, first, "ok");
        }
        catch (Exception ex) when (ex is SocketException or FormatException or OperationCanceledException)
        {
            cancellationToken.ThrowIfCancellationRequested();
            stopwatch.Stop();
            return new DnsResult(false, 0, stopwatch.ElapsedMilliseconds, null, ex.Message);
        }
    }

    private static byte[] BuildQuery(string host)
    {
        var labels = host.Trim().TrimEnd('.').Split('.', StringSplitOptions.RemoveEmptyEntries);
        using var stream = new MemoryStream();

        // Header.
        var id = (ushort)Random.Shared.Next(1, ushort.MaxValue);
        stream.WriteByte((byte)(id >> 8));
        stream.WriteByte((byte)(id & 0xFF));
        stream.WriteByte(0x01); // QR=0, recursion desired.
        stream.WriteByte(0x00);
        WriteUInt16(stream, 1); // QDCOUNT
        WriteUInt16(stream, 0); // ANCOUNT
        WriteUInt16(stream, 0); // NSCOUNT
        WriteUInt16(stream, 0); // ARCOUNT

        // Question.
        foreach (var label in labels)
        {
            var bytes = System.Text.Encoding.ASCII.GetBytes(label);
            stream.WriteByte((byte)bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        stream.WriteByte(0x00); // end of name
        WriteUInt16(stream, 1); // QTYPE = A
        WriteUInt16(stream, 1); // QCLASS = IN

        return stream.ToArray();
    }

    private static (int Count, string? First, int RCode) ParseAnswers(byte[] buffer)
    {
        if (buffer.Length < 12)
        {
            return (0, null, -1);
        }

        var rcode = buffer[3] & 0x0F;
        var questionCount = (buffer[4] << 8) | buffer[5];
        var answerCount = (buffer[6] << 8) | buffer[7];

        var offset = 12;

        // Skip the question section.
        for (var q = 0; q < questionCount; q++)
        {
            offset = SkipName(buffer, offset);
            offset += 4; // QTYPE + QCLASS
        }

        var found = 0;
        string? first = null;

        for (var a = 0; a < answerCount && offset + 10 <= buffer.Length; a++)
        {
            offset = SkipName(buffer, offset);
            if (offset + 10 > buffer.Length)
            {
                break;
            }

            var type = (buffer[offset] << 8) | buffer[offset + 1];
            var rdLength = (buffer[offset + 8] << 8) | buffer[offset + 9];
            offset += 10;

            if (type == 1 && rdLength == 4 && offset + 4 <= buffer.Length)
            {
                found++;
                first ??= $"{buffer[offset]}.{buffer[offset + 1]}.{buffer[offset + 2]}.{buffer[offset + 3]}";
            }

            offset += rdLength;
        }

        return (found, first, rcode);
    }

    private static int SkipName(byte[] buffer, int offset)
    {
        while (offset < buffer.Length)
        {
            var length = buffer[offset];
            if (length == 0)
            {
                return offset + 1;
            }

            if ((length & 0xC0) == 0xC0)
            {
                // Compression pointer occupies two bytes.
                return offset + 2;
            }

            offset += length + 1;
        }

        return offset;
    }

    private static void WriteUInt16(Stream stream, ushort value)
    {
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)(value & 0xFF));
    }
}
