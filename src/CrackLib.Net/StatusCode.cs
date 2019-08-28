namespace CrackLib.Net
{
	/// <summary>
	/// The error code response from a CrackLib check.
	/// </summary>
	public enum StatusCode
	{
		/// <summary>
		/// Password is too short. </summary>
		Short,
		/// <summary>
		/// Not enough different characters. </summary>
		Different,
		/// <summary>
		/// All whitespace. </summary>
		Whitespace,
		/// <summary>
		/// Based on dictionary word. </summary>
		Dictionary,
        /// <summary>
        /// Password is acceptable
        /// </summary>
        Ok
	}

}