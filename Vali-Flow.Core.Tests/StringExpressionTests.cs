using Xunit;
using FluentAssertions;
using Vali_Flow.Core.Builder;

namespace Vali_Flow.Core.Tests;

public class StringExpressionTests
{
    private static Product MakeProduct(string? name = "Test") =>
        new(name, 10m, 1, true, DateTime.UtcNow, new List<string>());

    // 1. IsNotNullOrEmpty
    [Fact]
    public void IsNotNullOrEmpty_MatchesNonNullNonEmpty_RejectsNullOrEmpty()
    {
        var filter = new ValiFlow<Product>()
            .IsNotNullOrEmpty(p => p.Name)
            .Build();

        filter.Compile()(MakeProduct("Hello")).Should().BeTrue();
        filter.Compile()(MakeProduct(null)).Should().BeFalse();
        filter.Compile()(MakeProduct("")).Should().BeFalse();
    }

    // 2. IsNullOrEmpty
    [Fact]
    public void IsNullOrEmpty_MatchesNullOrEmpty_RejectsNonEmpty()
    {
        var filter = new ValiFlow<Product>()
            .IsNullOrEmpty(p => p.Name)
            .Build();

        filter.Compile()(MakeProduct(null)).Should().BeTrue();
        filter.Compile()(MakeProduct("")).Should().BeTrue();
        filter.Compile()(MakeProduct("Hello")).Should().BeFalse();
    }

    // 3. MinLength(3)
    [Fact]
    public void MinLength_3_MatchesGte3Chars_RejectsShorter()
    {
        var filter = new ValiFlow<Product>()
            .MinLength(p => p.Name, 3)
            .Build();

        filter.Compile()(MakeProduct("abc")).Should().BeTrue();
        filter.Compile()(MakeProduct("abcd")).Should().BeTrue();
        filter.Compile()(MakeProduct("ab")).Should().BeFalse();
        filter.Compile()(MakeProduct("")).Should().BeFalse();
    }

    // 4. MaxLength(5)
    [Fact]
    public void MaxLength_5_MatchesLte5Chars_RejectsLonger()
    {
        var filter = new ValiFlow<Product>()
            .MaxLength(p => p.Name, 5)
            .Build();

        filter.Compile()(MakeProduct("hello")).Should().BeTrue();
        filter.Compile()(MakeProduct("hi")).Should().BeTrue();
        filter.Compile()(MakeProduct("toolong")).Should().BeFalse();
    }

    // 5. LengthBetween(2, 5)
    [Fact]
    public void LengthBetween_2_5_MatchesWithinRange_RejectsOutside()
    {
        var filter = new ValiFlow<Product>()
            .LengthBetween(p => p.Name, 2, 5)
            .Build();

        filter.Compile()(MakeProduct("hi")).Should().BeTrue();
        filter.Compile()(MakeProduct("hey")).Should().BeTrue();
        filter.Compile()(MakeProduct("hello")).Should().BeTrue();
        filter.Compile()(MakeProduct("h")).Should().BeFalse();
        filter.Compile()(MakeProduct("toolong")).Should().BeFalse();
    }

    // 6. ExactLength(4)
    [Fact]
    public void ExactLength_4_MatchesExactLengthOnly()
    {
        var filter = new ValiFlow<Product>()
            .ExactLength(p => p.Name, 4)
            .Build();

        filter.Compile()(MakeProduct("abcd")).Should().BeTrue();
        filter.Compile()(MakeProduct("abc")).Should().BeFalse();
        filter.Compile()(MakeProduct("abcde")).Should().BeFalse();
    }

    // 7. IsEmail
    [Fact]
    public void IsEmail_MatchesValidEmail_RejectsInvalid()
    {
        var filter = new ValiFlow<Product>()
            .IsEmail(p => p.Name)
            .Build();

        filter.Compile()(MakeProduct("user@example.com")).Should().BeTrue();
        filter.Compile()(MakeProduct("not-an-email")).Should().BeFalse();
        filter.Compile()(MakeProduct("missing@domain")).Should().BeFalse();
    }

    // 8. IsJson
    [Fact]
    public void IsJson_MatchesValidJson_RejectsInvalid()
    {
        var filter = new ValiFlow<Product>()
            .IsJson(p => p.Name)
            .Build();

        filter.Compile()(MakeProduct("{\"key\":\"value\"}")).Should().BeTrue();
        filter.Compile()(MakeProduct("[1,2,3]")).Should().BeTrue();
        filter.Compile()(MakeProduct("not json")).Should().BeFalse();
        filter.Compile()(MakeProduct("{invalid}")).Should().BeFalse();
    }

    // 9. IsBase64
    [Fact]
    public void IsBase64_MatchesValidBase64_RejectsInvalid()
    {
        var filter = new ValiFlow<Product>()
            .IsBase64(p => p.Name)
            .Build();

        // "Hello" in Base64 is "SGVsbG8=" (length 8, divisible by 4)
        filter.Compile()(MakeProduct("SGVsbG8=")).Should().BeTrue();
        filter.Compile()(MakeProduct("not-base64!")).Should().BeFalse();
        filter.Compile()(MakeProduct("abc")).Should().BeFalse();
    }

    // 9b. Regresión: Base64 con longitud no múltiplo de 4 debe rechazarse
    [Fact]
    public void IsBase64_LengthNotMultipleOf4_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().IsBase64(p => p.Name).Build();

        // "abcde" tiene 5 chars → longitud % 4 != 0 → no es Base64 válido
        filter.Compile()(MakeProduct("abcde")).Should().BeFalse();
        // "a" → 1 char → rechazado
        filter.Compile()(MakeProduct("a")).Should().BeFalse();
        // "ab" → 2 chars sin padding → rechazado (necesita "ab==")
        filter.Compile()(MakeProduct("ab")).Should().BeFalse();
        // "ab==" → 4 chars, padding correcto → aceptado
        filter.Compile()(MakeProduct("ab==")).Should().BeTrue();
    }

    // 10. IsGuid
    [Fact]
    public void IsGuid_MatchesValidGuidString_RejectsNonGuid()
    {
        var filter = new ValiFlow<Product>()
            .IsGuid(p => p.Name)
            .Build();

        filter.Compile()(MakeProduct("550e8400-e29b-41d4-a716-446655440000")).Should().BeTrue();
        filter.Compile()(MakeProduct("not-a-guid")).Should().BeFalse();
        filter.Compile()(MakeProduct("12345678")).Should().BeFalse();
    }

    // 11. IsUrl
    [Fact]
    public void IsUrl_MatchesHttpHttpsUrls_RejectsInvalid()
    {
        var filter = new ValiFlow<Product>()
            .IsUrl(p => p.Name)
            .Build();

        filter.Compile()(MakeProduct("https://www.example.com")).Should().BeTrue();
        filter.Compile()(MakeProduct("http://example.com")).Should().BeTrue();
        filter.Compile()(MakeProduct("not a url")).Should().BeFalse();
        filter.Compile()(MakeProduct("ftp://example.com")).Should().BeFalse();
    }

    // 12. IsUpperCase
    [Fact]
    public void IsUpperCase_MatchesAllCaps_RejectsMixed()
    {
        var filter = new ValiFlow<Product>()
            .IsUpperCase(p => p.Name)
            .Build();

        filter.Compile()(MakeProduct("HELLO")).Should().BeTrue();
        filter.Compile()(MakeProduct("Hello")).Should().BeFalse();
        filter.Compile()(MakeProduct("hello")).Should().BeFalse();
    }

    // 13. IsLowerCase
    [Fact]
    public void IsLowerCase_MatchesAllLowercase_RejectsMixed()
    {
        var filter = new ValiFlow<Product>()
            .IsLowerCase(p => p.Name)
            .Build();

        filter.Compile()(MakeProduct("hello")).Should().BeTrue();
        filter.Compile()(MakeProduct("Hello")).Should().BeFalse();
        filter.Compile()(MakeProduct("HELLO")).Should().BeFalse();
    }

    // 14. HasOnlyDigits
    [Fact]
    public void HasOnlyDigits_Matches12345_RejectsAbc()
    {
        var filter = new ValiFlow<Product>()
            .HasOnlyDigits(p => p.Name)
            .Build();

        filter.Compile()(MakeProduct("12345")).Should().BeTrue();
        filter.Compile()(MakeProduct("abc")).Should().BeFalse();
        filter.Compile()(MakeProduct("abc123")).Should().BeFalse();
    }

    // 15. HasOnlyLetters
    [Fact]
    public void HasOnlyLetters_MatchesAbc_RejectsAbc123()
    {
        var filter = new ValiFlow<Product>()
            .HasOnlyLetters(p => p.Name)
            .Build();

        filter.Compile()(MakeProduct("abc")).Should().BeTrue();
        filter.Compile()(MakeProduct("abc123")).Should().BeFalse();
        filter.Compile()(MakeProduct("123")).Should().BeFalse();
    }

    // 16. IsTrimmed
    [Fact]
    public void IsTrimmed_MatchesNonPaddedStrings_RejectsPadded()
    {
        var filter = new ValiFlow<Product>()
            .IsTrimmed(p => p.Name)
            .Build();

        filter.Compile()(MakeProduct("hello")).Should().BeTrue();
        filter.Compile()(MakeProduct(" hello")).Should().BeFalse();
        filter.Compile()(MakeProduct("hello ")).Should().BeFalse();
        filter.Compile()(MakeProduct(" hello ")).Should().BeFalse();
    }

    // 17. EqualsIgnoreCase("hello")
    [Fact]
    public void EqualsIgnoreCase_Hello_CaseInsensitiveMatch()
    {
        var filter = new ValiFlow<Product>()
            .EqualsIgnoreCase(p => p.Name, "hello")
            .Build();

        filter.Compile()(MakeProduct("hello")).Should().BeTrue();
        filter.Compile()(MakeProduct("HELLO")).Should().BeTrue();
        filter.Compile()(MakeProduct("Hello")).Should().BeTrue();
        filter.Compile()(MakeProduct("world")).Should().BeFalse();
    }

    // 18. IsNullOrWhiteSpace
    [Fact]
    public void IsNullOrWhiteSpace_MatchesNullEmptyWhitespace()
    {
        var filter = new ValiFlow<Product>()
            .IsNullOrWhiteSpace(p => p.Name)
            .Build();

        filter.Compile()(MakeProduct(null)).Should().BeTrue();
        filter.Compile()(MakeProduct("")).Should().BeTrue();
        filter.Compile()(MakeProduct("   ")).Should().BeTrue();
        filter.Compile()(MakeProduct("hello")).Should().BeFalse();
    }

    // 19. IsNotNullOrWhiteSpace
    [Fact]
    public void IsNotNullOrWhiteSpace_RejectsWhitespaceOnly()
    {
        var filter = new ValiFlow<Product>()
            .IsNotNullOrWhiteSpace(p => p.Name)
            .Build();

        filter.Compile()(MakeProduct("hello")).Should().BeTrue();
        filter.Compile()(MakeProduct("   ")).Should().BeFalse();
        filter.Compile()(MakeProduct(null)).Should().BeFalse();
        filter.Compile()(MakeProduct("")).Should().BeFalse();
    }

    // RegexMatch tests
    [Fact]
    public void RegexMatch_MatchingValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().RegexMatch(p => p.Name, @"^\d{5}$").Build().Compile();
        filter(MakeProduct("12345")).Should().BeTrue();
    }

    [Fact]
    public void RegexMatch_NonMatchingValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().RegexMatch(p => p.Name, @"^\d{5}$").Build().Compile();
        filter(MakeProduct("hello")).Should().BeFalse();
    }

    [Fact]
    public void RegexMatch_NullValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().RegexMatch(p => p.Name, @"^\d{5}$").Build().Compile();
        filter(MakeProduct(null)).Should().BeFalse();
    }

    // HasLettersAndNumbers tests
    [Fact]
    public void HasLettersAndNumbers_AlphanumericValue_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().HasLettersAndNumbers(p => p.Name).Build().Compile();
        filter(MakeProduct("abc123")).Should().BeTrue();
    }

    [Fact]
    public void HasLettersAndNumbers_DigitsOnly_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().HasLettersAndNumbers(p => p.Name).Build().Compile();
        filter(MakeProduct("12345")).Should().BeFalse();
    }

    [Fact]
    public void HasLettersAndNumbers_LettersOnly_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().HasLettersAndNumbers(p => p.Name).Build().Compile();
        filter(MakeProduct("abcde")).Should().BeFalse();
    }

    // HasSpecialCharacters tests
    [Fact]
    public void HasSpecialCharacters_StringWithSpecialChar_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().HasSpecialCharacters(p => p.Name).Build().Compile();
        filter(MakeProduct("hello@world")).Should().BeTrue();
    }

    [Fact]
    public void HasSpecialCharacters_PureAlpha_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().HasSpecialCharacters(p => p.Name).Build().Compile();
        filter(MakeProduct("hello")).Should().BeFalse();
    }

    // IsPhoneNumber tests
    [Fact]
    public void IsPhoneNumber_ValidE164_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().IsPhoneNumber(p => p.Name).Build().Compile();
        filter(MakeProduct("+15551234567")).Should().BeTrue();
    }

    [Fact]
    public void IsPhoneNumber_TooShort_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().IsPhoneNumber(p => p.Name).Build().Compile();
        filter(MakeProduct("+12")).Should().BeFalse();
    }

    [Fact]
    public void IsPhoneNumber_NullValue_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().IsPhoneNumber(p => p.Name).Build().Compile();
        filter(MakeProduct(null)).Should().BeFalse();
    }

    // NotJson tests
    [Fact]
    public void NotJson_ValidJson_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().NotJson(p => p.Name).Build().Compile();
        filter(MakeProduct("{\"key\":\"value\"}")).Should().BeFalse();
    }

    [Fact]
    public void NotJson_InvalidJson_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().NotJson(p => p.Name).Build().Compile();
        filter(MakeProduct("plain text")).Should().BeTrue();
    }

    // NotBase64 tests
    [Fact]
    public void NotBase64_ValidBase64_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().NotBase64(p => p.Name).Build().Compile();
        filter(MakeProduct("SGVsbG8=")).Should().BeFalse();
    }

    [Fact]
    public void NotBase64_InvalidBase64_ReturnsTrue()
    {
        var filter = new ValiFlow<Product>().NotBase64(p => p.Name).Build().Compile();
        filter(MakeProduct("not!base64")).Should().BeTrue();
    }

    // ── EqualsIgnoreCase OrdinalIgnoreCase regression ─────────────────────────

    [Fact]
    public void EqualsIgnoreCase_OrdinalSemantics_DifferentCase_ReturnsTrue()
    {
        // Arrange
        var filter = new ValiFlow<Product>()
            .EqualsIgnoreCase(p => p.Name, "hello")
            .Build().Compile();

        // Act & Assert
        filter(MakeProduct("HELLO")).Should().BeTrue();
    }

    [Fact]
    public void EqualsIgnoreCase_NullValue_ReturnsFalse()
    {
        // Arrange
        var filter = new ValiFlow<Product>()
            .EqualsIgnoreCase(p => p.Name, "hello")
            .Build().Compile();

        // Act & Assert
        filter(MakeProduct(null)).Should().BeFalse();
    }

    // Null string edge cases for length methods
    [Fact]
    public void MinLength_NullString_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().MinLength(p => p.Name, 3).Build().Compile();
        filter(MakeProduct(null)).Should().BeFalse();
    }

    [Fact]
    public void MaxLength_NullString_ReturnsTrue()
    {
        // MaxLength treats null as permissive (null satisfies any length upper bound)
        var filter = new ValiFlow<Product>().MaxLength(p => p.Name, 5).Build().Compile();
        filter(MakeProduct(null)).Should().BeTrue();
    }

    [Fact]
    public void ExactLength_NullString_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().ExactLength(p => p.Name, 3).Build().Compile();
        filter(MakeProduct(null)).Should().BeFalse();
    }

    // LengthBetween boundary: exactly max+1 chars
    [Fact]
    public void LengthBetween_MaxPlusOneLengthString_ReturnsFalse()
    {
        var filter = new ValiFlow<Product>().LengthBetween(p => p.Name, 2, 5).Build().Compile();
        filter(MakeProduct("toolon")).Should().BeFalse(); // 6 chars, one over max of 5
    }

    private record TwoFieldEntity(string? Title, string? Body);

    // 20. Contains multi-selector regression — node sharing must not prevent
    //     correct evaluation when 2+ selectors and 2+ search terms are used.
    [Fact]
    public void Contains_MultiSelector_MultiTerm_NoNodeSharing()
    {
        var selectors = new List<System.Linq.Expressions.Expression<Func<TwoFieldEntity, string?>>>
        {
            e => e.Title,
            e => e.Body,
        };

        var filter = new ValiFlow<TwoFieldEntity>()
            .Contains("hello world", selectors)
            .Build()
            .Compile();

        // Both terms present in Title
        filter(new TwoFieldEntity("hello world", null)).Should().BeTrue();
        // Terms split across Title and Body
        filter(new TwoFieldEntity("hello", "world")).Should().BeTrue();
        // Only first term present in Title
        filter(new TwoFieldEntity("hello", null)).Should().BeTrue();
        // Neither term present
        filter(new TwoFieldEntity("foo", "bar")).Should().BeFalse();
        // Null everywhere
        filter(new TwoFieldEntity(null, null)).Should().BeFalse();
    }
}

public class NewStringValidatorTests
{
    private record StringEntity(string? Value);
    private static StringEntity Make(string? v) => new(v);

    // IsCreditCard
    [Theory]
    [InlineData("4111111111111111")]   // Visa 16
    [InlineData("4012888888881881")]   // Visa 16
    [InlineData("5500005555555559")]   // Mastercard
    [InlineData("371449635398431")]    // Amex 15
    [InlineData("6011111111111117")]   // Discover
    public void IsCreditCard_ValidNumbers_ReturnsTrue(string card)
    {
        new ValiFlow<StringEntity>().IsCreditCard(e => e.Value)
            .IsValid(Make(card)).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1234")]
    [InlineData("not-a-card")]
    public void IsCreditCard_InvalidValues_ReturnsFalse(string? card)
    {
        new ValiFlow<StringEntity>().IsCreditCard(e => e.Value)
            .IsValid(Make(card)).Should().BeFalse();
    }

    [Fact]
    public void IsCreditCard_NullSelector_Throws()
        => Assert.Throws<ArgumentNullException>(() =>
            new ValiFlow<StringEntity>().IsCreditCard(null!));

    // IsIPv4
    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("0.0.0.0")]
    [InlineData("255.255.255.255")]
    [InlineData("10.0.0.1")]
    public void IsIPv4_ValidAddresses_ReturnsTrue(string ip)
    {
        new ValiFlow<StringEntity>().IsIPv4(e => e.Value)
            .IsValid(Make(ip)).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("256.0.0.1")]
    [InlineData("192.168.1")]
    [InlineData("::1")]
    [InlineData("not-an-ip")]
    public void IsIPv4_InvalidValues_ReturnsFalse(string? ip)
    {
        new ValiFlow<StringEntity>().IsIPv4(e => e.Value)
            .IsValid(Make(ip)).Should().BeFalse();
    }

    [Fact]
    public void IsIPv4_NullSelector_Throws()
        => Assert.Throws<ArgumentNullException>(() =>
            new ValiFlow<StringEntity>().IsIPv4(null!));

    // IsIPv6
    [Theory]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
    [InlineData("::1")]
    [InlineData("fe80::1")]
    [InlineData("::")]
    public void IsIPv6_ValidAddresses_ReturnsTrue(string ip)
    {
        new ValiFlow<StringEntity>().IsIPv6(e => e.Value)
            .IsValid(Make(ip)).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("192.168.1.1")]
    [InlineData("not-an-ip")]
    [InlineData("gggg::1")]
    public void IsIPv6_InvalidValues_ReturnsFalse(string? ip)
    {
        new ValiFlow<StringEntity>().IsIPv6(e => e.Value)
            .IsValid(Make(ip)).Should().BeFalse();
    }

    [Fact]
    public void IsIPv6_NullSelector_Throws()
        => Assert.Throws<ArgumentNullException>(() =>
            new ValiFlow<StringEntity>().IsIPv6(null!));

    // IsHexColor
    [Theory]
    [InlineData("#fff")]
    [InlineData("#FFF")]
    [InlineData("#ffffff")]
    [InlineData("#FFFFFF")]
    [InlineData("#1a2b3c")]
    [InlineData("#ABC")]
    public void IsHexColor_ValidColors_ReturnsTrue(string color)
    {
        new ValiFlow<StringEntity>().IsHexColor(e => e.Value)
            .IsValid(Make(color)).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("fff")]
    [InlineData("#gggggg")]
    [InlineData("#12345")]
    [InlineData("red")]
    public void IsHexColor_InvalidValues_ReturnsFalse(string? color)
    {
        new ValiFlow<StringEntity>().IsHexColor(e => e.Value)
            .IsValid(Make(color)).Should().BeFalse();
    }

    [Fact]
    public void IsHexColor_NullSelector_Throws()
        => Assert.Throws<ArgumentNullException>(() =>
            new ValiFlow<StringEntity>().IsHexColor(null!));

    // IsSlug
    [Theory]
    [InlineData("hello-world")]
    [InlineData("my-post-title")]
    [InlineData("abc123")]
    [InlineData("a")]
    [InlineData("hello-world-123")]
    public void IsSlug_ValidSlugs_ReturnsTrue(string slug)
    {
        new ValiFlow<StringEntity>().IsSlug(e => e.Value)
            .IsValid(Make(slug)).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("-hello")]
    [InlineData("hello-")]
    [InlineData("Hello-World")]
    [InlineData("hello world")]
    [InlineData("hello--world")]
    public void IsSlug_InvalidValues_ReturnsFalse(string? slug)
    {
        new ValiFlow<StringEntity>().IsSlug(e => e.Value)
            .IsValid(Make(slug)).Should().BeFalse();
    }

    [Fact]
    public void IsSlug_NullSelector_Throws()
        => Assert.Throws<ArgumentNullException>(() =>
            new ValiFlow<StringEntity>().IsSlug(null!));
}

public class WildcardAndIsOneOfTests
{
    private record StringEntity(string? Value);
    private static StringEntity Make(string? v) => new(v);

    // MatchesWildcard tests

    [Fact]
    public void MatchesWildcard_StarSuffix_MatchesPrefix_ReturnsTrue()
    {
        new ValiFlow<StringEntity>().MatchesWildcard(e => e.Value, "order*")
            .IsValid(Make("order123")).Should().BeTrue();
    }

    [Fact]
    public void MatchesWildcard_StarSuffix_NoMatch_ReturnsFalse()
    {
        new ValiFlow<StringEntity>().MatchesWildcard(e => e.Value, "order*")
            .IsValid(Make("invoice123")).Should().BeFalse();
    }

    [Fact]
    public void MatchesWildcard_StarPrefix_MatchesSuffix_ReturnsTrue()
    {
        new ValiFlow<StringEntity>().MatchesWildcard(e => e.Value, "*test")
            .IsValid(Make("mytest")).Should().BeTrue();
    }

    [Fact]
    public void MatchesWildcard_QuestionMark_SingleChar_ReturnsTrue()
    {
        new ValiFlow<StringEntity>().MatchesWildcard(e => e.Value, "a?c")
            .IsValid(Make("abc")).Should().BeTrue();
    }

    [Fact]
    public void MatchesWildcard_QuestionMark_TooShort_ReturnsFalse()
    {
        new ValiFlow<StringEntity>().MatchesWildcard(e => e.Value, "a?c")
            .IsValid(Make("ac")).Should().BeFalse();
    }

    [Fact]
    public void MatchesWildcard_NullValue_ReturnsFalse()
    {
        new ValiFlow<StringEntity>().MatchesWildcard(e => e.Value, "order*")
            .IsValid(Make(null)).Should().BeFalse();
    }

    [Fact]
    public void MatchesWildcard_NullPattern_ThrowsArgumentNullException()
        => Assert.Throws<ArgumentNullException>(() =>
            new ValiFlow<StringEntity>().MatchesWildcard(e => e.Value, null!));

    // IsOneOf tests

    [Fact]
    public void IsOneOf_MatchingValue_ReturnsTrue()
    {
        new ValiFlow<StringEntity>().IsOneOf(e => e.Value, new[] { "A", "B", "C" })
            .IsValid(Make("b")).Should().BeTrue();
    }

    [Fact]
    public void IsOneOf_NonMatchingValue_ReturnsFalse()
    {
        new ValiFlow<StringEntity>().IsOneOf(e => e.Value, new[] { "A", "B", "C" })
            .IsValid(Make("D")).Should().BeFalse();
    }

    [Fact]
    public void IsOneOf_NullValue_ReturnsFalse()
    {
        new ValiFlow<StringEntity>().IsOneOf(e => e.Value, new[] { "A", "B", "C" })
            .IsValid(Make(null)).Should().BeFalse();
    }

    [Fact]
    public void IsOneOf_EmptyValues_ThrowsArgumentException()
        => Assert.Throws<ArgumentException>(() =>
            new ValiFlow<StringEntity>().IsOneOf(e => e.Value, Array.Empty<string>()));

    [Fact]
    public void IsOneOf_CaseSensitive_ExactMatch_ReturnsTrue()
    {
        new ValiFlow<StringEntity>().IsOneOf(e => e.Value, new[] { "A", "B", "C" }, StringComparison.Ordinal)
            .IsValid(Make("A")).Should().BeTrue();
    }

    [Fact]
    public void IsOneOf_CaseSensitive_WrongCase_ReturnsFalse()
    {
        new ValiFlow<StringEntity>().IsOneOf(e => e.Value, new[] { "A", "B", "C" }, StringComparison.Ordinal)
            .IsValid(Make("a")).Should().BeFalse();
    }
}
