using System;

namespace NDbPortal.Sample.Web.LtvDev.Views

{
	public class VideosView
	{
		public long Id {get; set;}

		public string VideoUrl {get; set;}

		public int VideoSourceId {get; set;}

		public DateTime AddedDate {get; set;}

		public long AddedBy {get; set;}

		public string VideoSourceName {get; set;}

		public string VideoCategoryIds {get; set;}

		public string VideoTitle {get; set;}

		public string VideoDetails {get; set;}

		public int LocaleId {get; set;}

		public string AddedByName {get; set;}

		public long ViewsCount {get; set;}

	}
}
