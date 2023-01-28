using System.ComponentModel.DataAnnotations;

namespace Models{
    public class Radnik{
        [Key]
        public int ID{get;set;}
        public string Ime{get;set;}
        public string Prezime{get;set;}
        public string Pozicija{get;set;}
        public int BrutoPlata{get;set;}
    }
}