using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using ClosedXML.Excel;
namespace Evidencija.Controllers{

[ApiController]
[Route("[controller]")]
public class EvidencijaController : ControllerBase{

        public EvidencijaContext Context { get; set; }
        public EvidencijaController(EvidencijaContext context){
            Context=context;
        }
        [HttpGet]
        [Route("GetRadnici")]

        public async Task<ActionResult> GetRadnici(){
            try{
            return Ok(await Context.Radnici.ToListAsync());
            }
            catch(Exception e){
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("PostRadnik")]

        public async Task<ActionResult> PostRadnik(string ime,string prezime,string pozicija, int brutoPlata){
            try{
                var radnik=new Radnik{
                    Ime=ime,
                    Prezime=prezime,
                    Pozicija=pozicija,
                    BrutoPlata=brutoPlata
                };
                await Context.Radnici.AddAsync(radnik);
                await Context.SaveChangesAsync();
                return Ok("Super");
                }
            catch(Exception e){
                return BadRequest(e.Message);
            }
        }
        [HttpGet]
        [Route("DetaljniUvidjaj")]
        public async Task<ActionResult> DetaljniUvidjaj(int id){
                try{
                    var radnik=Context.Radnici.Where(p=>p.ID==id).FirstOrDefault();
                    if(radnik==null)
                        return BadRequest("Radnik sa trazenim id-jem ne postoji!");
                    var PIO=0.24*radnik.BrutoPlata;
                    var Osiguranje=0.0515*radnik.BrutoPlata;
                    var Doprinosi=0.0075*radnik.BrutoPlata;
                    var Porez=0.1*radnik.BrutoPlata;
                    var NetoPlata=radnik.BrutoPlata-PIO-Osiguranje-Doprinosi-Porez;

                    return Ok(new {
                        brutoPlata=radnik.BrutoPlata,
                        pio=PIO,
                        osiguranje=Osiguranje,
                        doprinosi=Doprinosi,
                        porez=Porez,
                        netoPlata=NetoPlata
                    });
                }
                catch(Exception e){
                    return BadRequest(e.Message);
                }
            }
            [HttpGet]
            [Route("ExcelIzvestajSvi")]

            public  async Task<IActionResult> ExcelIzvestajSvi(){//u teoriji bi trebalo da radi
                try{
                var WorkBook= new XLWorkbook();
                var WorkSheet=WorkBook.Worksheets.Add("Radnici");

                    WorkSheet.Cell("A1").Value="Ime";
                    WorkSheet.Cell("B1").Value="Prezime";
                    WorkSheet.Cell("C1").Value="Pozicija";
                    WorkSheet.Cell("D1").Value="BrutoPlata";

                    var Radnici=await Context.Radnici.ToListAsync();
                    var count=Radnici.Count();
                    int j=2;
                    for(int i=0;i<count;i++){
                        WorkSheet.Cell("A"+j).Value=Radnici[i].Ime;
                        WorkSheet.Cell("B"+j).Value=Radnici[i].Prezime;
                        WorkSheet.Cell("C"+j).Value=Radnici[i].Pozicija;
                        WorkSheet.Cell("D"+j).Value=Radnici[i].BrutoPlata;
                        j++;
                    }

                    var stream=new System.IO.MemoryStream();
                    WorkBook.SaveAs(stream);
                    var content=stream.ToArray();
                    return File(content,"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TESTSTSTS.xlsx");

                }
                catch(Exception e){
                    return BadRequest(e.Message);
                }
            }
        }
    }


