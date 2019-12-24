using System;
using System.Collections.Generic;
using System.Text;

namespace LazyLizard.LogicHandler
{
    public class ResponseImageId
    {
        public string id { get; set; }
    }


    public class Location
    {
        public string type { get; set; }
        public string country { get; set; }
        public string city { get; set; }
        public string state { get; set; }
    }

    public class Name
    {
        public string first { get; set; }
        public string company { get; set; }
        public string surname { get; set; }
    }

    public class Profile
    {
        public string avatar_url { get; set; }
        public string gender { get; set; }
        public List<Location> locations { get; set; }
        public Name name { get; set; }
        public string about { get; set; }
        public string skype_handle { get; set; }
        public string language { get; set; }
        public string website { get; set; }
    }

    public class Source
    {
        public string type { get; set; }
        public string subtype { get; set; }
        public DateTime time { get; set; }
    }

    public class RelationshipHistory
    {
        public List<Source> sources { get; set; }
    }

    public class Info
    {
        public List<string> capabilities { get; set; }
        public string trusted { get; set; }
        public string type { get; set; }
    }

    public class Agent
    {
        public List<string> capabilities { get; set; }
        public string trust { get; set; }
        public string type { get; set; }
        public Info info { get; set; }
    }

    public class Contact
    {
        public string person_id { get; set; }
        public string mri { get; set; }
        public string display_name { get; set; }
        public string display_name_source { get; set; }
        public Profile profile { get; set; }
        public bool authorized { get; set; }
        public bool blocked { get; set; }
        public bool @explicit { get; set; }
        public DateTime creation_time { get; set; }
        public RelationshipHistory relationship_history { get; set; }
        public Agent agent { get; set; }
    }

    public class ContactInfo
    {
        public List<Contact> contacts { get; set; }
        public string scope { get; set; }
        public int count { get; set; }
    }

}
