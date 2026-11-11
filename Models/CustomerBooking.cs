namespace HolidayMaker.Models;

public sealed record CustomerBooking(
    int BookingId,
    string AccommodationName,
    string RoomNumber,
    DateTime StartDate,
    DateTime EndDate,
    int NumberOfGuests
);
