using System.Text;
using Microsoft.AspNetCore.Http;
using RaceDay.Web.Helpers;

namespace RaceDay.Web.Tests;

public class FormatAndUploadTests
{
    [Theory]
    [InlineData(1, "1st")] [InlineData(2, "2nd")] [InlineData(3, "3rd")] [InlineData(4, "4th")]
    [InlineData(11, "11th")] [InlineData(12, "12th")] [InlineData(13, "13th")]
    [InlineData(21, "21st")] [InlineData(47, "47th")] [InlineData(112, "112th")]
    public void Ordinal_formats_correctly(int n, string expected) => Assert.Equal(expected, Format.Ordinal(n));

    [Fact]
    public void Time_formats_hh_mm_ss() => Assert.Equal("01:23:45", Format.Time(new TimeSpan(1, 23, 45)));

    [Theory]
    [InlineData("Confirmed", "status-confirmed")]
    [InlineData("pending", "status-pending")]
    [InlineData("Cancelled", "status-cancelled")]
    [InlineData("???", "status-other")]
    public void StatusClass_maps_statuses(string status, string css) => Assert.Equal(css, Format.StatusClass(status));

    [Theory]
    [InlineData("/Home/Index", true)]
    [InlineData("//evil.com", false)]
    [InlineData("/\\evil.com", false)]
    [InlineData("https://evil.com", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsLocalUrl_blocks_open_redirects(string? url, bool expected) => Assert.Equal(expected, Format.IsLocalUrl(url));

    private static IFormFile File(string name, long size)
    {
        var bytes = new byte[Math.Min(size, 10)];
        var f = new FormFile(new MemoryStream(bytes), 0, size, "file", name);
        return f;
    }

    [Fact] public void Upload_rejects_null() => Assert.NotNull(UploadValidator.Validate(null));
    [Fact] public void Upload_rejects_empty() => Assert.NotNull(UploadValidator.Validate(File("a.png", 0)));
    [Fact] public void Upload_rejects_wrong_type() => Assert.NotNull(UploadValidator.Validate(File("virus.exe", 100)));
    [Fact] public void Upload_rejects_oversize() => Assert.NotNull(UploadValidator.Validate(File("big.png", UploadValidator.MaxBytes + 1)));
    [Fact] public void Upload_accepts_valid_image() => Assert.Null(UploadValidator.Validate(File("banner.JPG", 1000)));
}
