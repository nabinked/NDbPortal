using System;

namespace NDbPortal.Sample.Web.LtvDev.Views

{
    public class NewsArticlesView
    {
        public long Id { get; set; }

        public int NewsStatusId { get; set; }

        public long PublisherId { get; set; }

        public int AuthorId { get; set; }

        public DateTime PublishedDate { get; set; }

        public string CanonicalUrl { get; set; }

        public string Source { get; set; }

        public DateTime UpdatedTs { get; set; }

        public string Tags { get; set; }

        public string StatusName { get; set; }

        public int[] NewsCategoryIds { get; set; }

        public string Heading { get; set; }

        public string Content { get; set; }

        public int LocaleId { get; set; }

        public string LocaleName { get; set; }

        public string AuthorName { get; set; }

        public string PublisherName { get; set; }

        public long ViewsCount { get; set; }

        public long ThumbnailImageId { get; set; }

        public string ThumbnailImageName { get; set; }

    }
}
