namespace OpenStack.Swift
{
	using System;

    /// <summary>
    /// ClientException is thrown by <see><cref>Openstack.Swift.Client</cref></see>
    /// </summary>
    public class ClientException : ApplicationException
	{
        /// <summary>
        /// Returns the HTTP Status Code from the response server.
        /// </summary>
		public readonly int Status;

        /// <summary>
        /// Initializes a new instance of the <see><cref>Openstack.Swift.ClientException</cref></see> class.
        /// </summary>
        /// <param name='message'>
        /// A <see cref="System.String"/> The exception message.
        /// </param>
        /// <param name='status'>
        /// A <see cref="System.Int32"/> that is the status code from the responding server.
        /// </param>
        public ClientException(string message, int status) : base(message)
		{
			Status = status;
		}
	}
}

