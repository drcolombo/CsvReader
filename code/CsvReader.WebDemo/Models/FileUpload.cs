using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using CsvReader.WebDemo.Attributes;
using Microsoft.AspNetCore.Http;

namespace CsvReader.WebDemo.Models
{
    public class FileUpload
    {
        [Required(ErrorMessage = "Please select a file.")]
        [DataType(DataType.Upload)]
        //[MaxFileSize(5 * 1024 * 1024)]
        [AllowedExtensions(new[] { ".csv" })]
        public IFormFile FormFile { get; set; }
        public char Separator { get; set; }

        public DataTable Data { get; set; }

        public FileUpload()
        {
            Data = new DataTable();
            Separator = ',';
        }
    }
}
