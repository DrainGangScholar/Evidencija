using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using ClosedXML.Excel;
using Newtonsoft.Json;
using System.Text.Json;


namespace Evidencija.Controllers{
    [ApiController]
    [Route("[controller]")]
    public class EvidencijaController : ControllerBase{
            [NonAction]
            public void Obracun(ref double BrutoPlata, ref double PIO, ref double Osiguranje, ref double Doprinosi, ref double Porez, ref double NetoPlata,int val ){
           
                var client = new HttpClient();
                var response = client.GetAsync("https://v6.exchangerate-api.com/v6/94a988704774a72fd90feb65/latest/RSD").Result;
                    if(response.IsSuccessStatusCode){
                        string content = response.Content.ReadAsStringAsync().Result;
                        var json = JsonConvert.DeserializeObject<dynamic>(content)!;
                        double value=1;
                        if (val == 1)
                        {
                            value = (json["conversion_rates"]["USD"]).ToObject<double>();
                        }
                        else{
                            value = json["conversion_rates"]["EUR"].ToObject<double>();
                        }
                    BrutoPlata=BrutoPlata*value;
                    PIO=0.24*BrutoPlata;
                    Osiguranje=0.0515*BrutoPlata;
                    Doprinosi=0.0075*BrutoPlata;
                    Porez=0.1*BrutoPlata;
                    NetoPlata=BrutoPlata-PIO-Osiguranje-Doprinosi-Porez;     
                }
            }
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
            public async Task<ActionResult> DetaljniUvidjaj(int id,int val){
                    try{
                        var radnik=await Context.Radnici.Where(p=>p.ID==id).FirstOrDefaultAsync();
                        if(radnik==null)
                            return BadRequest("Radnik sa trazenim id-jem ne postoji!");
                        double bP=radnik.BrutoPlata;
                        double PIO=0;
                        double Osiguranje=0;
                        double Doprinosi=0;
                        double Porez=0;
                        double NetoPlata=0;
                        string Valuta="RSD";
                        if(val==1)
                            Valuta="USD";
                        else if(val==2)
                            Valuta="EUR";
                        Obracun(ref bP, ref  PIO, ref  Osiguranje, ref  Doprinosi, ref  Porez, ref  NetoPlata,val);
                        int BP=(int) bP;
                        Radnik _radnik=new Radnik{
                            Ime=radnik.Ime,
                            Prezime=radnik.Prezime,
                            Pozicija=radnik.Pozicija,
                            BrutoPlata=BP
                        };
                        return Ok(new {
                            Radnik=_radnik,
                            pio=PIO,
                            osiguranje=Osiguranje,
                            doprinosi=Doprinosi,
                            porez=Porez,
                            netoPlata=NetoPlata,
                            valuta=Valuta
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

                    var stream=new MemoryStream();
                    WorkBook.SaveAs(stream);
                    var content=stream.ToArray();
                    return File(content,"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TESTSTSTS.xlsx");

                }
                catch(Exception e){
                    return BadRequest(e.Message);
                }
            }
            [HttpGet]
            [Route("CsvIzvestajSvi")]
            public async Task<ActionResult>CsvIzvestajSvi(){
                try{
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.WriteLine("Name,Age,Address");
                var Radnici=await Context.Radnici.ToListAsync();
                foreach (Radnik radnik in Radnici)
                {
                    writer.WriteLine($"{radnik.Ime},{radnik.Prezime},{radnik.Pozicija},{radnik.BrutoPlata}");
                }
                writer.Flush();
                stream.Position = 0;
                return File(stream, "text/csv", "data.csv");
                }
                catch(Exception e){
                    return BadRequest(e.Message);
                }
            }
        }
    }


