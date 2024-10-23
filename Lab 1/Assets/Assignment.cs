
/*
This RPG data streaming assignment was created by Fernando Restituto with 
pixel RPG characters created by Sean Browning.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//adding
using System.Text;
using System.IO;
using System;
using System.Diagnostics.CodeAnalysis;


#region Assignment Instructions

/*  Hello!  Welcome to your first lab :)

Wax on, wax off.

    The development of saving and loading systems shares much in common with that of networked gameplay development.  
    Both involve developing around data which is packaged and passed into (or gotten from) a stream.  
    Thus, prior to attacking the problems of development for networked games, you will strengthen your abilities to develop solutions using the easier to work with HD saving/loading frameworks.

    Try to understand not just the framework tools, but also, 
    seek to familiarize yourself with how we are able to break data down, pass it into a stream and then rebuild it from another stream.


Lab Part 1

    Begin by exploring the UI elements that you are presented with upon hitting play.
    You can roll a new party, view party stats and hit a save and load button, both of which do nothing.
    You are challenged to create the functions that will save and load the party data which is being displayed on screen for you.

    Below, a SavePartyButtonPressed and a LoadPartyButtonPressed function are provided for you.
    Both are being called by the internal systems when the respective button is hit.
    You must code the save/load functionality.
    Access to Party Character data is provided via demo usage in the save and load functions.

    The PartyCharacter class members are defined as follows.  */

public partial class PartyCharacter
{
    public int classID;

    public int health;
    public int mana;

    public int strength;
    public int agility;
    public int wisdom;

    public LinkedList<int> equipment;

}

/*
    Access to the on screen party data can be achieved via …..

    Once you have loaded party data from the HD, you can have it loaded on screen via …...

    These are the stream reader/writer that I want you to use.
    https://docs.microsoft.com/en-us/dotnet/api/system.io.streamwriter
    https://docs.microsoft.com/en-us/dotnet/api/system.io.streamreader

    Alright, that’s all you need to get started on the first part of this assignment, here are your functions, good luck and journey well!
*/


#endregion


#region Assignment Part 1

static public class AssignmentPart1
{
    static public void SavePartyButtonPressed()
    {
        //get the directory
        //but instead we are going to make a string that hard puts it in
        string filePath = "C:\\Users\\pawfu\\Desktop\\Multiplayer Systems\\testing.txt";
        //string filePath = " D:\\School\\Multiplayer Systems\\Game3110_Labs\\Lab 1";
        //write each directory name to a file
        using (StreamWriter sw = new StreamWriter(filePath))
        {
            //impliment this
            foreach (PartyCharacter pc in GameContent.partyCharacters)
            {
                //Debug.Log("PC class id == " + pc.classID);
                //make it write the line
                sw.WriteLine(pc.classID + "," +
                    pc.health + "," +
                    pc.mana + "," +
                    pc.strength + "," +
                    pc.agility + "," +
                    pc.wisdom);

                //if the equipment count is above 0 AKA there is equipment
                if (pc.equipment.Count > 0)
                {
                    //join each line of it together in a row using string.Join
                    sw.WriteLine(string.Join(", ", pc.equipment));
                }
                else 
                {
                    //if there is none say no
                    sw.WriteLine("no equipment");
                }
                //adds an extrs line to seperate the two
                sw.WriteLine("....");
            }
        }
        //show where it is being saved to
        Debug.Log("PC saved to " + filePath);
    }

    static public void LoadPartyButtonPressed()
    {
        //first we find the file path
        string filePath = "C:\\Users\\pawfu\\Desktop\\Multiplayer Systems\\testing.txt";

        //ok so we want to make it so when this button is pressed it loads whatever was saved
        //so this clears whats on the screen first
        try
        {
            GameContent.partyCharacters.Clear();
            // Create an instance of StreamReader to read from a file.
            // The using statement also closes the StreamReader.

            //using Encoding.UTF8 to ensure that non-ASCII characters are properly read
            using (StreamReader sr = new StreamReader(filePath, Encoding.UTF8))
            {
                string line;
                PartyCharacter pc = null;

                // Read and display lines from the file until the end of
                // the file is reached.
                while((line = sr.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                    Debug.Log(line);

                    if(line == "....")
                    {
                        if(pc != null)
                        {
                            GameContent.partyCharacters.AddLast(pc);
                            pc = null;
                        }
                        continue;
                    }
                    //this is where we will create a character from the first line of info
                    if(pc == null)
                    {
                        Debug.Log("== null");

                        //use int.Parse to convert the string into an int
                        string[] charParse = line.Split(',');

                        //need to identify 6 lines to read all data from
                        if(charParse.Length == 6)
                        {
                            pc = new PartyCharacter(
                                int.Parse(charParse[0]),
                                int.Parse(charParse[1]),
                                int.Parse(charParse[2]),
                                int.Parse(charParse[3]),
                                int.Parse(charParse[4]),
                                int.Parse(charParse[5])
                                );
                            Debug.Log("Parsed");
                        }
                    }
                    else
                    {
                        //now move onto the equipment
                        if(line != "no equipment")
                        {
                            //set up the equipment data
                            string[] equipmentSplit = line.Split(",");

                            //for each will make sure that it goes through each line.. so for each one add last
                            foreach(string equipment in equipmentSplit)
                            {
                                //use parse to read from the string
                                pc.equipment.AddLast(int.Parse(equipment));
                                Debug.Log("Equip parsed");

                            }
                        }
                    }
                }
                if (pc != null) 
                {
                    GameContent.partyCharacters.AddLast(pc);
                }
            }
        }
        //throw an exception if this is wrong
        catch (Exception e)
        {
            // Let the user know what went wrong.
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
        }
        //refresh
        GameContent.RefreshUI();
        Debug.Log("Party characters loaded from " + filePath);
    }
}


#endregion


#region Assignment Part 2

//  Before Proceeding!
//  To inform the internal systems that you are proceeding onto the second part of this assignment,
//  change the below value of AssignmentConfiguration.PartOfAssignmentInDevelopment from 1 to 2.
//  This will enable the needed UI/function calls for your to proceed with your assignment.
static public class AssignmentConfiguration
{
    //part 1
    //public const int PartOfAssignmentThatIsInDevelopment = 1;

    //part 2
    public const int PartOfAssignmentThatIsInDevelopment = 2;
}

/*

In this part of the assignment you are challenged to expand on the functionality that you have already created.  
    You are being challenged to save, load and manage multiple parties.
    You are being challenged to identify each party via a string name (a member of the Party class).

To aid you in this challenge, the UI has been altered.  

    The load button has been replaced with a drop down list.  
    When this load party drop down list is changed, LoadPartyDropDownChanged(string selectedName) will be called.  
    When this drop down is created, it will be populated with the return value of GetListOfPartyNames().

    GameStart() is called when the program starts.

    For quality of life, a new SavePartyButtonPressed() has been provided to you below.

    An new/delete button has been added, you will also find below NewPartyButtonPressed() and DeletePartyButtonPressed()

Again, you are being challenged to develop the ability to save and load multiple parties.
    This challenge is different from the previous.
    In the above challenge, what you had to develop was much more directly named.
    With this challenge however, there is a much more predicate process required.
    Let me ask you,
        What do you need to program to produce the saving, loading and management of multiple parties?
        What are the variables that you will need to declare?
        What are the things that you will need to do?  
    So much of development is just breaking problems down into smaller parts.
    Take the time to name each part of what you will create and then, do it.

Good luck, journey well.

*/

//okay so starting off i need to make a list of things i need
//ability to save new party names
//ability to get the list of party names and read it back


static public class AssignmentPart2
{
    //need data indentifiers for both PC and equip
    const int PCSaveDataIdentifier = 0;
    const int PCEquipmentSaveDataIndentifier = 1;

    const int LastIndexIdentifier = 1;
    const int IndexAndNameIdentifier = 2;

    //use my new class yippie
    static LinkedList<NameAndIndex> nameAndIndices;
    //
    static int lastIndexUsed;
    //create a list of party names
    static List<string> listOfPartyNames;
    //connect it the file path
    //const string filePath = "C:\\Users\\pawfu\\Desktop\\Multiplayer Systems\\testing.txt";

    //i think i need a new one
    const string filePath = "SavedData/testing.txt";

    //start game
    static public void GameStart()
    {
        //init new LL to store the saved party names
        nameAndIndices = new LinkedList<NameAndIndex>();

        //so double checking the file path is real lol
        if (File.Exists(Application.dataPath + +Path.DirectorySeparatorChar + filePath))
        {
            //init the stream reader
            //read file
            //load the saved party names from the txt
            using (StreamReader sr = new StreamReader(Application.dataPath + Path.DirectorySeparatorChar + filePath))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    //so change this 
                    //listOfPartyNames.Add(line);

                    //process each line from the file
                    string[] charParse = line.Split(',');
                    int identifier = int.Parse(charParse[0]);

                    //handle diff types of data
                    //so if it matches this, load the last used index
                    if (identifier == LastIndexIdentifier)
                    {
                        lastIndexUsed = int.Parse(charParse[1]);
                    }
                    //if it matches this, the line contains a party name AND its index
                    else if(identifier == IndexAndNameIdentifier)
                    {
                        //second val is index, 3rd is party name
                        nameAndIndices.AddLast(new NameAndIndex(int.Parse(charParse[1]), charParse[2]));
                    }

                }
            }
        }
        //create a list of party names for UI
        listOfPartyNames = new List<string>();

        //it loops through anf the party name is added to the list
        foreach (NameAndIndex nameAndIndex in nameAndIndices)
        {
            listOfPartyNames.Add(nameAndIndex.name);
        }

        //refresh UI
        GameContent.RefreshUI();
    }

    //getter for the list
    static public List<string> GetListOfPartyNames()
    {
        return listOfPartyNames;
    }

    //so we will use this to load what was saved
    static public void LoadPartyDropDownChanged(string selectedName)
    {
        GameContent.partyCharacters.Clear();

        //get the index we need
        //starts off by saying that there is no valid index yet
        int indexWeNeed = -1;

        //go through everything
        foreach(NameAndIndex nameAndIndex in nameAndIndices)
        {
            //if the name we are looking for matches the selected name
            if(nameAndIndex.name == selectedName)
            {
                //change it to the index we need
                indexWeNeed = nameAndIndex.index;
            }
        }
        //after we find it, if it is a valid number
        if (indexWeNeed != -1)
        {
            //read the party data
            ReadPartyData(indexWeNeed);
        }
        
    }

    static public void SavePartyButtonPressed()
    {
        //so first when we save we want to save to a new party name
        //so we make a new party by accessing the array
        //this is what gives us the ability to type and write the PN
        string partyName = GameContent.GetPartyNameFromInput();
        bool isNewName = true;
        //next make sure there is a party name
        //tell the user it cant be empty
        if(string.IsNullOrEmpty(partyName)) 
        {
            Debug.Log("Party name cannot be empty!");
            return;
        }

        //go through a loop of all the name and index
        foreach (NameAndIndex nameAndIndex in nameAndIndices)
        {
            if (nameAndIndex.name == partyName)
            {
                SavePartyName(Application.dataPath + Path.DirectorySeparatorChar + nameAndIndex.index + ".txt");
                //now we need to make sure that we are making sure you cant save the same name
                isNewName = false;
            }
        }
        //now if it is a new name
        if(isNewName) 
        {
            //add up one in the last index used
            lastIndexUsed++;
            Debug.Log($"index is now {lastIndexUsed}");
            //save the party
            SavePartyName(Application.dataPath + Path.DirectorySeparatorChar + lastIndexUsed + ".txt");
            //add the new name and indx
            nameAndIndices.AddLast(new NameAndIndex(lastIndexUsed, partyName));
            //and add to the list of party names
            listOfPartyNames.Add(partyName);
        }
        Debug.Log($"Party '{partyName}' saved to {filePath} with index of {lastIndexUsed}");
        GameContent.RefreshUI();

        //call save index here
        SaveIndexFile();
    }

    static public void DeletePartyButtonPressed()
    {
        //getting a bool to flag true and false
        bool isDeleting = false;

        string partyNameToDelete = GameContent.GetPartyNameFromInput();

        //debugging
        if (string.IsNullOrEmpty(partyNameToDelete))
        {
            Debug.Log("Need a party in order to delete!");
            return;
        }
        else
        {
            Debug.Log("Look at you! You can delete this!");
        }

        NameAndIndex partyIndexToDelete = null;
        //looping through everything
        foreach(NameAndIndex nameAndIndex in nameAndIndices)
        {
            //if the party name we want to delete is the same as the name we r looking for
            if(nameAndIndex.name == partyNameToDelete) 
            {
                StreamWriter sw = new StreamWriter(Application.dataPath + Path.DirectorySeparatorChar + nameAndIndex.name + ".txt");
                sw.Dispose();

                //make the party to delete the name and index
                partyIndexToDelete = nameAndIndex;
                isDeleting = true;
                break;
            }
        }
        if(isDeleting)
        {
            //remove the nameAndIndices and list of party name
            nameAndIndices.Remove(partyIndexToDelete);
            listOfPartyNames.Remove(partyNameToDelete);

            Debug.Log("Deleted file");


            //refresh UI
            GameContent.RefreshUI();

            //call save index file to update the IF
            SaveIndexFile();
        }
        else
        {
            Debug.Log($"Party '{partyNameToDelete}' not found.");
        }
    }

    //okay so i want to make a list of my saves that i can go through by using a data "key" if you will
    static public void LoadPC(LinkedList<string> data)
    {
        GameContent.partyCharacters.Clear();
        PartyCharacter pc = null;

        //go through and check
        foreach (string line in data)
        {
            Debug.Log($"Processing line: {line}"); // Log the line being processed
            string[] charParse = line.Split(',');

            if (charParse.Length < 7)
            {
                Debug.LogWarning($"Not enough data elements in line: {line}");
                continue; // Skip this line
            }

            int identifier = int.Parse(charParse[0]);

            //add something to check to see if the indentifier matches the character data
            if(identifier == PCSaveDataIdentifier)
            {
               pc = new PartyCharacter(
               int.Parse(charParse[1]), // classID
               int.Parse(charParse[2]), // health
               int.Parse(charParse[3]), // mana
               int.Parse(charParse[4]), // strength
               int.Parse(charParse[5]), // agility
               int.Parse(charParse[6])  // wisdom
               );

                GameContent.partyCharacters.AddLast(pc);
            }
            //now do the same but with the equipment identifier
            else if(identifier == PCEquipmentSaveDataIndentifier) 
            {
                //make sure there is even a pc there
                if (pc != null)
                {
                    // add the equip
                    //pc.equipment.AddLast(int.Parse(charParse[1]));
                    GameContent.partyCharacters.Last.Value.equipment.AddLast(int.Parse(charParse[1]));
                }
                else
                {
                    Debug.LogWarning("No character found.");
                }
            }
        }

        GameContent.RefreshUI();
        Debug.Log("Party loaded successfully.");
    }

    //now i want to be able to read the party data
    static private void ReadPartyData(int dataToLoad)
    {
        //new SR
        StreamReader sr = new StreamReader(Application.dataPath + Path.DirectorySeparatorChar + dataToLoad + ".txt");

        LinkedList<string> partyData = new LinkedList<string>();
        string line;

        //while there r lines read them all
        while ((line = sr.ReadLine()) != null)
        {
            //find the line we r looking for
            partyData.AddLast(line);
        }
        sr.Close();
        //load it
        LoadPC(partyData);
    }

    //i need to save the index file as well
    public static void SaveIndexFile()
    {
        //so we are writing to the new path
        //so first get the path
        StreamWriter sw = new StreamWriter(Application.dataPath + Path.DirectorySeparatorChar + filePath);

        //writing the line
        sw.WriteLine(LastIndexIdentifier + "," + lastIndexUsed);

        //loop to add
        //so go through the class and write a new line
        foreach (NameAndIndex nameAndIndex in nameAndIndices)
        {
            sw.WriteLine(IndexAndNameIdentifier + "," + nameAndIndex.index + "," + nameAndIndex.name);
        }
        sw.Close();
    }

    //going to create a func that will save the party names
    //no mumbo jumbo goin on here
    static public void SavePartyName(string fileName)
    {
        //so instead we want to call on a linked list and save the line to that
        LinkedList<string> partyData = SavePartyDataString();

        using (StreamWriter sw = new StreamWriter(fileName))
        {
            foreach (string partyName in partyData)
            {
                sw.WriteLine(partyName);
            }
            sw.Close();
        }
        
    }

    //gnna make a function that holds the saving of the data
    //but this will be a linked list
    static private LinkedList<string> SavePartyDataString()
    {
        LinkedList<string> data = new LinkedList<string>();

        foreach (PartyCharacter pc in GameContent.partyCharacters)
        {
            //Debug.Log("PC class id == " + pc.classID);
            //make it write the line
            data.AddLast(PCSaveDataIdentifier + "," +
                pc.classID + "," +
                pc.health + "," +
                pc.mana + "," +
                pc.strength + "," +
                pc.agility + "," +
                pc.wisdom);

            //now do the same with the equipment
            foreach (int Equipment in pc.equipment)
            {
                data.AddLast(PCEquipmentSaveDataIndentifier + "," + Equipment);
            }
        }
        return data;
    }

}

//gnna make a new class that holds Name and Index
//that way i can store the party string name and the int index
public class NameAndIndex
{
    public string name;
    public int index;

    //construct
    public NameAndIndex(int Index, string Name)
    {
        name = Name;
        index = Index;
    }
}

#endregion


