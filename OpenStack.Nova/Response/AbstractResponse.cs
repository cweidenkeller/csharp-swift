namespace OpenStack.Nova
{
	using System;

	public class AbstractResponse
	{
		/// <summary>
		/// Headers from the container request
		/// </summary>
	    public readonly Dictionary<string, string> Headers;
		/// <summary>
		/// The Status code
		/// </summary>
		public readonly int Status;
		/// <summary>
		/// The status description
		/// </summary>
		public readonly string Reason;

		public AbstractResponse ()
		{	
		
		}
	}
}

