namespace UserQL.GraphQL
{
    public record UserToken
    (
        string? Token,
        string? Expired,
        string? Message
    );
}
