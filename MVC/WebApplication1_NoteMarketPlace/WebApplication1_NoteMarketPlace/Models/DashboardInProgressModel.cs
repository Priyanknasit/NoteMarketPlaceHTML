using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1_NoteMarketPlace.Models
{
    public class DashboardInProgressModel
    {
        public int ID { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public string Status { get; set; }

        public DateTime CreatedDate { get; set; }

    }

    public class DashboardInPublishedModel
    {
        public int ID { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public string SellType { get; set; }

        public decimal Price { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}