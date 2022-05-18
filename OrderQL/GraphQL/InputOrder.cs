using Models.Models;

namespace OrderQL.GraphQL
{
    public record InputOrder
    (
        int? Id,
        string Code,
        int UserId, 
        //int? CourierId, 
        //DateTime StartDate, 
        //DateTime? EndDate,
        //bool? Status,
        List<OrderDetail> OrderDetails
     );
}
