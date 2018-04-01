namespace NDbPortal.Sample.Web.LtvDev

{
	public class MenuItem
	{
		public long Id {get; set;}

		public int ParentId {get; set;}

		public string Label {get; set;}

		public string Link {get; set;}

		public bool IsHidden {get; set;}

	}
}
