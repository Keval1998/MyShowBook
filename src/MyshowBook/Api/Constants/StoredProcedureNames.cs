namespace MyShowBook.Api.Constants;

public static class StoredProcedureNames
{
    public const string GetUserForLogin = "sp_get_user_for_login";
    public const string RegisterUser = "sp_register_user";
    public const string CreateShow = "sp_create_show";
    public const string GetShow = "sp_get_show";
    public const string ListShows = "sp_list_shows";
    public const string CreateReservation = "sp_create_reservation";
    public const string CancelReservation = "sp_cancel_reservation";
    public const string GetAvailableSeatsMetrics = "sp_get_available_seats_metrics";
}