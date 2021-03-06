﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.ServiceModel.Syndication;
using System.Net;

namespace ConsolaAPIGoogleAlert
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Conectando con google...");
            Google.GoogleAlertAPI ga = new Google.GoogleAlertAPI("leonardo.diaz.corvalan@gmail.com", "Nuevaetica1232123");

            Console.WriteLine("Capturando alertas...");
            List<Google.GoogleAlertAPI.Alert> list = ga.getAlerts();

            string usuarioSQL = ConfigurationSettings.AppSettings["usuarioSQL"];
            string servidorSQL = ConfigurationSettings.AppSettings["servidorSQL"];
            string passwordSQL = ConfigurationSettings.AppSettings["passwordSQL"];
            string bd = ConfigurationSettings.AppSettings["bdSQL"];

            string cnnStr = "Data Source=" + servidorSQL + ";Initial Catalog=" + bd + ";User ID=" + usuarioSQL + ";Password=" + passwordSQL + "";

            SqlConnection cnn = new SqlConnection(cnnStr);
            Console.WriteLine("Abriendo conexion con SQL...");
            cnn.Open();
            
            Console.WriteLine("Cantidad de alertas = "+ list.Count().ToString());
            foreach (var alert in list)
            {
                string query = alert.query;
                string feed = alert.feedUrl;
                string deliveryto = alert.deliveryTo.ToString();
                string howMany = alert.howMany.ToString();
                string howOften = alert.howOften.ToString();
                string idAlerta = alert.id;
                string lenguaje = alert.language.ToString();
                string region = alert.region.ToString();

                
                // alert object
                //stpIngresarAlerta(@id varchar(50), @deliveryto varchar(50), @feed varchar(500), @howMany varchar(50), @howOften varchar(50),@languaje varchar(50), @query varchar(50), @region varchar(50))
                string queryControl = "exec stp_IngresarAlerta'" + idAlerta + "','" + deliveryto + "','" + feed + "','" + howMany + "','" + howOften + "','" + lenguaje + "','" + query + "','" + region + "'";
                SqlCommand command = new SqlCommand(queryControl, cnn);
                command.ExecuteNonQuery();

                
                using (var feedReader = XmlReader.Create(feed))
                {
                    var feedContent = SyndicationFeed.Load(feedReader);

                    if (null == feedContent) return;

                    foreach (var item in feedContent.Items)
                    {

                        Console.WriteLine("Item Title: " + item.Title.Text);
                        string titulo = item.Title.Text;
                        string id = item.Id;
                        
                        int cate = item.Categories.Count();
                        string tituloLink = string.Empty;
                        string uri = string.Empty;
                        string autor = string.Empty;
                        DateTimeOffset dateFecha = item.PublishDate;
                        string fecha = dateFecha.DateTime.ToShortDateString();
                        string contenido = ((System.ServiceModel.Syndication.TextSyndicationContent)item.Content).Text;
                        string tipo = item.Content.Type;
                 

                        Console.WriteLine("Item Links");
                        foreach (var link in item.Links)
                        {
                            Console.WriteLine("Link Title: " + link.Title);
                            tituloLink = link.Title;
                            Console.WriteLine("URI: " + link.Uri);
                            uri = link.Uri.OriginalString;
                            Console.WriteLine("RelationshipType: " + link.RelationshipType);
                            Console.WriteLine("MediaType: " + link.MediaType);
                            Console.WriteLine("Length: " + link.Length);
                            Console.WriteLine("Base Uri: " + link.BaseUri);

                            //Console.WriteLine(lin);
                        }

                        
                        foreach (var itemAutor in feedContent.Authors)
                        {
                            autor = itemAutor.Name;
                        }
                        
                        string queryNoticias = "exec stp_IngresoNoticia'" + id + "','" + idAlerta + "','" + titulo + "','" + autor + "','" + contenido + "','" + uri + "','" + uri + "','" + fecha + "'";
                        Console.WriteLine("Ingresando Noticia... query="+ queryNoticias);
                        SqlCommand commandNoticias = new SqlCommand(queryNoticias, cnn);
                        commandNoticias.ExecuteNonQuery();

                    }
                }
                
            }
            Console.ReadKey();
        }
    }
    
}
