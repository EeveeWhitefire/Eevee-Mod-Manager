using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using EeveexModManager.Classes;
using EeveexModManager.Classes.DatabaseClasses;

namespace EeveexModManager.Controls
{
    public class Mod_Control : Control
    {
        public bool ModState
        {
            get { return AssociatedMod.Active; }
            set {
                AssociatedMod.Active = value;
                Update();
            }
        }
        public string ModName
        {
            get { return AssociatedMod.Name; }
            set
            {
                AssociatedMod.Name = value;
                Update();
            }
        }
        public string FileName
        {
            get { return AssociatedMod.ModFileName; }
            set
            {
                AssociatedMod.ModFileName = value;
                Update();
            }
        }
        public string ModAuthor
        {
            get { return AssociatedMod.Author; }
            set
            {
                AssociatedMod.Author = value;
                Update();
            }
        }
        public string ModVersion
        {
            get { return AssociatedMod.Version; }
            set
            {
                AssociatedMod.Version = value;
                Update();
            }
        }
        public string ModId
        {
            get { return AssociatedMod.Id; }
            set
            {
                AssociatedMod.Id = value;
                Update();
            }
        }

        private DatabaseContext_Profile _db;
        public Mod AssociatedMod { get; set; }

        public Mod_Control(Mod m, DatabaseContext_Profile db)
        {
            AssociatedMod = m;
            _db = db;
        }
        
        public void Update()
        {
            _db.GetCollection<Mod>("mods").Update(AssociatedMod);
        }
    }
}
