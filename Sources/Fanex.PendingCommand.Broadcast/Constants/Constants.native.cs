namespace Fanex.PendingCommand.Broadcast.Constants;

public enum TicketType
{
    Running,
    Settled
}

public static class EmailType
{
    public const string Credit = "Credit";

    public const string Deposit = "Deposit";
}

public static class StatusType
{
    public const short Running = 0;
}

public static class ContentLanguage
{
    public const string English = "en";
    public const string Cantonese = "zh-TW";
    public const string Mandarin = "zh-CN";
    public const string NewMandarin = "zh-CHS";
}

public static class ActionName
{
    public const string Calculate = "Cal";

    public const string ReSettle = "ReSettle";

    public const string Refund = "Refund";
}

public static class MsgContentType
{
    public const short Running = 105;
    public const short Settled = 110;
}

public static class SportType
{
    public const int NumberGame = 161;
    public const int VirtualBetRadarMin = 190;
    public const int VirtualBetRadarMax = 199;
}