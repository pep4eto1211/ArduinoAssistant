using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;
using System.IO.Ports;

namespace ArduinoAssistantLib
{

    public class ProjectExistsException : Exception
    {
        private string m_message = "A project with the same name already exists.";
        public ProjectExistsException()
        {

        }

        public string message
        {
            get
            {
                return m_message;
            }
        }
    }

    public class PortOpenException : Exception
    {
        private string m_message = "Changes to the COM comunicator can't be made while the port is open.";

        public PortOpenException()
        {

        }

        public string Message1
        {
            get
            {
                return m_message;
            }
        }
    }

    public class Note
    {
        private string m_text;
        private string m_name;
        private Project m_noteProject;
        private Color m_backgroundcolor = Color.White;

        public Note(string name, string text, Project noteProject)
        {
            m_text = text;
            m_noteProject = noteProject;
        }

        public string Text
        {
            get
            {
                return m_text;
            }

            set
            {
                File.WriteAllText(m_noteProject.Directory + @"\Notes\" + m_name + ".aapd", value);
                m_text = value;
            }
        }

        public Project NoteProject
        {
            get
            {
                return m_noteProject;
            }
        }

        public Color Backgroundcolor
        {
            get
            {
                return m_backgroundcolor;
            }

            set
            {
                m_backgroundcolor = value;
            }
        }
    }

    public class Project
    {
        private string m_name;
        private string m_directory;
        private string m_description;
        private List<Note> m_notes = new List<Note>();
        private List<string> m_files = new List<string>();

        private Project(string projectName, string projectDirectory, string projectDescription)
        {
            m_name = projectName;
            m_directory = projectDirectory;
            m_description = projectDescription;
        }

        public string Name
        {
            get
            {
                return m_name;
            }

            set
            {
                m_name = value;
            }
        }

        public string Directory
        {
            get
            {
                return m_directory;
            }

            set
            {
                m_directory = value;
            }
        }

        public string Description
        {
            get
            {
                return m_description;
            }

            set
            {
                m_description = value;
            }
        }

        public List<Note> Notes
        {
            get
            {
                return m_notes;
            }

            set
            {
                m_notes = value;
            }
        }

        public List<string> Files
        {
            get
            {
                return m_files;
            }

            set
            {
                m_files = value;
            }
        }

        public static Project createNewProject(string projectName, string projectDirectory, string projectDescription)
        {
            if (!System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Arduino Assistant\"))
            {
                System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Arduino Assistant\");
            }
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Arduino Assistant\" + projectName + ".aapd"))
            {
                throw new ProjectExistsException();
            }
            else
            {
                File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Arduino Assistant\" + projectName + ".aapd",
                    projectDirectory + @"\" + projectName);
                System.IO.Directory.CreateDirectory(projectDirectory + @"\" + projectName);
                File.WriteAllText(projectDirectory + @"\" + projectName + @"\" + "Description.aapd", projectDescription);
                File.SetAttributes(projectDirectory + @"\" + projectName + @"\" + "Description.aapd", FileAttributes.Hidden);
                return new Project(projectName, projectDirectory + @"\" + projectName, projectDescription);
            }
        }

        public static List<Project> getAllProjects()
        {
            if (!System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Arduino Assistant\"))
            {
                System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Arduino Assistant\");
            }
            List<string> allProjectNames = System.IO.Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                @"\Arduino Assistant\").ToList<string>();
            List<Project> toReturn = new List<Project>();
            foreach (string singleFile in allProjectNames)
            {
                string projectDirectory = File.ReadAllText(singleFile);
                string projectName = singleFile.Substring(singleFile.LastIndexOf(@"\") + 1);
                projectName = projectName.Substring(0, projectName.Length - 5);
                string projectDescription = File.ReadAllText(projectDirectory + @"\Description.aapd");
                Project newProject = new Project(projectName, projectDirectory, projectDescription);

                if (System.IO.Directory.Exists(projectDirectory + @"\Notes\"))
                {
                    string[] allNotes = System.IO.Directory.GetFiles(projectDirectory + @"\Notes\");
                    foreach (string singleNote in allNotes)
                    {
                        string noteName = singleNote.Substring(singleNote.LastIndexOf(@"\"));
                        noteName = noteName.Substring(0, noteName.Length - 5);
                        Note newNote = new Note(noteName, File.ReadAllText(singleNote), newProject);
                        newProject.m_notes.Add(newNote);
                    }
                }

                if (System.IO.Directory.Exists(projectDirectory + @"\Sources\"))
                {
                    newProject.m_files = System.IO.Directory.GetFiles(projectDirectory + @"\Sources\").ToList<string>();
                }
                toReturn.Add(newProject);
            }

            return toReturn;
        }

    }

    public delegate void ComDataReceivedEventHandler(object source, ComDataReceivedEventArgs e);

    public class ComDataReceivedEventArgs : EventArgs
    {
        private string m_receivedData;
        private SerialDataReceivedEventArgs m_serialEventArgs;

        public ComDataReceivedEventArgs(string receivedData, SerialDataReceivedEventArgs e)
        {
            m_receivedData = receivedData;
            m_serialEventArgs = e;
        }

        public string ReceivedData
        {
            get
            {
                return m_receivedData;
            }
        }

        public SerialDataReceivedEventArgs SerialEventArgs
        {
            get
            {
                return m_serialEventArgs;
            }
        }
    }

    public class ComComunicator 
    {
        private string m_portName;
        private bool m_isPortOpen = false;
        private SerialPort port;
        public event ComDataReceivedEventHandler DataReceived;
        public ComComunicator(string portName)
        {
            m_portName = portName;
        }

        public string PortName
        {
            get
            {
                return m_portName;
            }

            set
            {
                if (!m_isPortOpen)
                {
                    m_portName = value;
                }
                else
                {
                    throw new PortOpenException();
                }
            }
        }

        public void startListening()
        {
            m_isPortOpen = true;
            port = new SerialPort(m_portName, 9600, Parity.None, 8, StopBits.One);
            port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            port.Open();
        }

        public void stopListening()
        {
            port.Close();
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            DataReceived(this, new ComDataReceivedEventArgs(port.ReadLine(), e));
        }
    }
}
