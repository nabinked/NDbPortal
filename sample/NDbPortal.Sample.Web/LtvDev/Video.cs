using System;

namespace NDbPortal.Sample.Web.LtvDev

{
	public class Video
	{
		public long Id {get; set;}

		public string VideoUrl {get; set;}

		public int VideoSourceId {get; set;}

		public DateTime AddedDate {get; set;}

		public long AddedBy {get; set;}

	}
}
