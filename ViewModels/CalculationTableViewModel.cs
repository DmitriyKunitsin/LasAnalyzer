using LasAnalyzer.Services.Graphics;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LasAnalyzer.ViewModels
{
    public class CalculationTableViewModel : ViewModelBase
    {
        private ObservableCollection<Person> people;
        public ObservableCollection<Person> People
        {
            get => people;
            set => this.RaiseAndSetIfChanged(ref people, value);
        }
        //public ObservableCollection<Person> People { get; }

        public CalculationTableViewModel()
        {
            var people = new List<Person>
            {
                new Person("Neil", "Armstrong"),
                new Person("Buzz", "Lightyear"),
                new Person("James", "Kirk")
            };
            People = new ObservableCollection<Person>(people);
        }
    }

    public class Person
    {
        //public string firstName;
        //public string FirstName
        //{
        //    get => firstName;
        //    set => this.RaiseAndSetIfChanged(ref firstName, value);
        //}
        //public string lastName;
        //public string LastName
        //{
        //    get => lastName;
        //    set => this.RaiseAndSetIfChanged(ref lastName, value);
        //}

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public Person(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
