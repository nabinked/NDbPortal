using System;

namespace NDbPortal.Sample.Web.LtvDev

{
	public class NewsArticle
	{
		public long Id {get; set;}

		public int NewsStatusId {get; set;}

		public long PublisherId {get; set;}

		public int AuthorId {get; set;}

		public DateTime PublishedDate {get; set;}

		public string CanonicalUrl {get; set;}

		public string Source {get; set;}

		public DateTime UpdatedTs {get; set;}

		public string Tags {get; set;}

	}
}
