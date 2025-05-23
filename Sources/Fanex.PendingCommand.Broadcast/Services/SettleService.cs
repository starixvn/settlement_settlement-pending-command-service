namespace Fanex.PendingCommand.Broadcast.Services;

public interface SettleService
{
    Task Settle();
}

public class SettleServiceImp : SettleService
{
    public async Task Settle()
    {
        //var batchTicketRequest = CreateBatchTicketRequest(request, site, resources);
        //result = await processService.Settle(batchTicketRequest);
    }
}