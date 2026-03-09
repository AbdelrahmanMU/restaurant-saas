namespace RestaurantSaas.Api.Domain.Enums;

public enum OrderStatus
{
    PendingAcceptance,
    Accepted,
    Preparing,
    ReadyForDispatch,
    PendingHandover,
    PickedUp,
    Delivered,
    Completed,
    Cancelled
}
