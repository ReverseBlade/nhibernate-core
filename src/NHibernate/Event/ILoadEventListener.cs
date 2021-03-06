using System.Threading.Tasks;
namespace NHibernate.Event
{
	/// <summary>
	/// Defines the contract for handling of load events generated from a session. 
	/// </summary>
	public interface ILoadEventListener
	{
		/// <summary> 
		/// Handle the given load event. 
		/// </summary>
		/// <param name="event">The load event to be handled. </param>
		/// <param name="loadType"></param>
		/// <returns> The result (i.e., the loaded entity). </returns>
		Task OnLoad(LoadEvent @event, LoadType loadType);
	}
}