using System;
using System.Collections.Generic;

namespace Xenvious.JSON.META
{

    public class Rootobject
    {
        public Content content { get; set; }
        public Users users { get; set; }
        public Crews crews { get; set; }
        public bool status { get; set; }
    }

    public class Content
    {
        public string id { get; set; }
        public int userId { get; set; }
        public string imgSrc { get; set; }
        public string category { get; set; }
        public DateTime createdDate { get; set; }
        public string name { get; set; }
        public string desc { get; set; }
        public Data data { get; set; }
        public object[] userTags { get; set; }
        public bool liked { get; set; }
        public bool disliked { get; set; }
        public int likeCount { get; set; }
        public int dislikeCount { get; set; }
        public int playedCount { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public bool bookmarked { get; set; }
        public bool played { get; set; }
        public string platform { get; set; }
    }

    public class Data
    {
    }

    public class Users
    {
        public User_Id user_id { get; set; }
    }

    public class User_Id
    {
        public int rockstarId { get; set; }
        public string nickname { get; set; }
        public int crewId { get; set; }
        public int crewRank { get; set; }
    }

    public class Crews
    {
        public Crews_Id crews_id { get; set; }
    }

    public class Crews_Id
    {
        public int id { get; set; }
        public string name { get; set; }
        public string tag { get; set; }
        public bool isPrivate { get; set; }
        public bool isFounder { get; set; }
        public string color { get; set; }
    }

}
