namespace UserQL.GraphQL
{
    public record InputProfile
    (
        int? Id,
        int UserId, 
        string Name, 
        string Address, 
        string City, 
        string Phone
    );
}
