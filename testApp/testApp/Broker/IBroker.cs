using System.Threading.Tasks;

namespace testApp.Broker;

public interface IBroker
{
	Task SendMessage(TaskItem task);
}