using System;
using System.Collections.Generic;
using Hl7.Fhir.Model; //FHIR specification model
using Hl7.Fhir.Rest; //rest API
using Hl7.Fhir.Serialization; //Convert to JSON

namespace CS_FHIR
{
public static class Program_v2
{
 
    private static readonly Dictionary<string, string> _fhirServers = new Dictionary<string, string>() //dict to choose from different servers
    {
        {"PublicVonk", "http://vonk.fire.ly"},
        {"PublicHapi", "http://hapi.fhir.org/baseR4"},
        {"Local", "http://localhost:8080/fhir"},
    };

    private static readonly string _fhirServer = _fhirServers["Local"]; //create the desired server connection
    
    
    /// <summary>
    /// Main entry point for program
    /// </summary>
    /// <param name="args"></param>
    static int Main(string[] args)                          // int type, return 0 for cmd line control - void also ok but returns nothing
    {
        FhirClient fhirClient = new FhirClient(_fhirServer) //create a client and connect to fhir server
        {
            PreferredFormat = ResourceFormat.Json,
            PreferredReturn = Prefer.ReturnRepresentation
        };

        List<Patient> patients = GetPatients(fhirClient); //call the function to populate the list

        System.Console.WriteLine($"Found {patients.Count} patients!"); //test if smth found
        
        return 0;
    }
    
    /// <summary>
    /// Create a list of Patients matching specified criteria
    /// </summary>
    /// <param name="fhirClient"></param>
    /// <param name="patientCriteria"></param>
    /// <param name="maxPatients">The max numb of patients to return (def:20)</param>
    /// <param name="onlyWithEncounters">Flag to only return patients with encounters (def:false)</param>
    /// <returns></returns>//  
    
        static List<Patient> GetPatients(
            FhirClient fhirClient,
            string[] patientCriteria = null,
            int maxPatients = 20,
            bool onlyWithEncounters = false)               //maxPatients to limit the search optional
        {
        List<Patient> patients = new List<Patient>();

        Bundle patientBundle;                           //create a bundle object (Fhir specification) and
        if ((patientCriteria == null)  || (patientCriteria.Length == 0))                 //make fhir search for patients filter by name = test
        {
        
        patientBundle = fhirClient.Search<Patient>(); // fhirClient.Search<Patient>(new string[]{"name=test"}); //command for filtered search
                                                                                     
        
        }
        else 
        {
            patientBundle = fhirClient.Search<Patient>(patientCriteria);
        }
       
        while (patientBundle != null)                                              
        {
                                                                                                                 //list all the patients in bundle               
            System.Console.WriteLine($"Patient BundleTotal: {patientBundle.Total} Entry count: {patientBundle.Entry.Count}"); //Print no of patients and patients in bundle
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

                if (onlyWithEncounters && (encounterBundle.Total == 0))
                {
                    continue;
                } 
                patients.Add(patient);                                 //add patients with encounters in list

                System.Console.WriteLine($"- Entry: {patients.Count,3}: {entry.FullUrl}");  //print the patientNumber and url
                System.Console.WriteLine($"- Id: {patient.Id}");                            //print patient id

                if (patient.Name.Count > 0)
                {
                    System.Console.WriteLine($" - Name: {patient.Name[0].ToString()}");  //patient.Name is a list check bundle specification
                } 

                if (encounterBundle.Total > 0)
                {
                     System.Console.WriteLine($" - Encounters Total: {encounterBundle.Total} Entry count: {encounterBundle.Entry.Count}");                                          
                }
            }
            if (patients.Count>=maxPatients)
            {
                break;
            }
            
            
        }
        if (patients.Count >= maxPatients)
        {
            break;
        }

            patientBundle = fhirClient.Continue(patientBundle);     //get more patients (results)
        }

        return patients;

    }

}
}
