using System.ComponentModel.DataAnnotations;

namespace DataFilter.Models
{
    public class Book : IDataFilter
    {
        public int Id { get; set; }
        [Display(Name = "书名")]
        public string Name { get; set; }
        public string UserName { get; set; }
    }
}
