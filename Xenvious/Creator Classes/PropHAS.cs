using System.Linq;

namespace Xenvious
{
    public class PropHAS
    {
        public int Id { get; set; }
        public int Time { get; set; }
        public int Prop { get; set; }
        public string Name { get; set; }

        public PropHAS(int id, int time, int prop)
        {
            Id = id;
            Time = time;
            Prop = prop;
            Name = MainWindow.ExistsInPropList(Prop) ? GTA.Editor.PropList.Where(x => x.Integer == Prop).Select(x => x.Name).First() : Prop.ToString();
        }

        public override string ToString()
        {
            return Prop.ToString();
        }
    }
}
