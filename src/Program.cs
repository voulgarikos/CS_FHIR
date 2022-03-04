using System;
using System.Collections.Generic;
using Hl7.Fhir.Model; //FHIR specification model
using Hl7.Fhir.Rest; //rest API
using Hl7.Fhir.Serialization; //Convert to JSON

namespace CS_FHIR
{
public static class Program
{
    private const string _fhirServer = "http://vonk.fire.ly"; //server from firely connecting to R4, change url to connect to other server
                                                            //program works with all servers
    static void Main(string[] args)
    {
        FhirClient fhirClient = new FhirClient(_fhirServer) //create a client and connect to fhir server
        {
            PreferredFormat = ResourceFormat.Json,
            PreferredReturn = Prefer.ReturnRepresentation
        };

        Bundle patientBundle = fhirClient.Search<Patient>(new string[]{"name=test"}); //create a bundle object (Fhir specification) and
                                                                                     //make fhir search for patients filter by name = test
        

        int patientNumber = 0;                                                       //var to count the patients in the bundle
        List<string> patientsWithEncounters = new List<string>();
        while (patientBundle != null)                                               //list all the patients in bundle
        {

            System.Console.WriteLine($"Total: {patientBundle.Total} Entry count: {patientBundle.Entry.Count}"); //Print no of patients and patients in bundle
            foreach (Bundle.EntryComponent entry in patientBundle.Entry)
        {
            
            

            if (entry.Resource != null)
            {
                Patient patient = (Patient)entry.Resource;
               // System.Console.WriteLine($" - {patient.Id,20}"); //print the patient id
                Bundle encounterBundle = fhirClient.Search<Encounter>(              //Create encounter bundle and make fhir search filter with patient id
                    new string[]
                    {
                    $"patient=Patient/{patient.Id}",
                    }  );

                if (encounterBundle.Total == 0)
                {
                    continue;
                } 
                patientsWithEncounters.Add(patient.Id);                                 //add patients with encounters in list

                System.Console.WriteLine($"- Entry: {patientNumber,3}: {entry.FullUrl}");  //print the patientNumber and url
                System.Console.WriteLine($"- Id: {patient.Id}");                            //print patient id

                if (patient.Name.Count > 0)
                {
                    System.Console.WriteLine($" - Name: {patient.Name[0].ToString()}");  //patient.Name is a list check bundle specification
                } 

                
                     System.Console.WriteLine($" - Encounters Total: {encounterBundle.Total} Entry count: {encounterBundle.Entry.Count}");                                          
            }

            patientNumber++;
            
        }
            patientBundle = fhirClient.Continue(patientBundle);
        }

        

    }

}
}
