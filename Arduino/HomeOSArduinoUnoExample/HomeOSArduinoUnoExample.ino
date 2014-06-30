/*
  HomeOSArduinoUnoExample. 
  
  Demonstrates how a HomeOS Arduino Uno device should respond to queries from the scout and drivers
  Leverages the Blink example to turns on an LED on for one second, then off for one second, repeatedly.
 
  This example code is in the public domain.
  
  All commands sent should have the form [<command>].   Code below expects '[' as start indicator and ']' as terminator of command
  Standard commands 
  [?] - send unique name of device using this format: HomeOSArduinoDevice_NameofDriver_YourOrganization_UniqueID   NameofDriver and Your Organization must match your Lab of Things driver name
  
  Use '[!' at beginning to tell driver you are sending error message.  For example [!Badly formatted command].
  
  Your driver should define commands to send and receive from Arduino. Most straightforward if each command starts with different character
 */
 
// Pin 13 has an LED connected on most Arduino boards.
// give it a name:
int led = 13;
int numCharRead = 0;
char incomingData[20];
int dummyValue = 0;

// the setup routine runs once when you press reset:
void setup() {                
  // initialize the digital pin as an output.
  pinMode(led, OUTPUT);  
  
  //Setup the serial port
 Serial.begin(9600);
}

// the loop routine runs over and over again forever:
void loop() {
   
 
  if (Serial.available() > 0) {
     numCharRead = Serial.readBytesUntil(']',  incomingData, 19);
     //buffer should contain some command with '[' at start and then command - it will not have terminator   
      processCommandsFromLoT(numCharRead);        
  }
  
  //A dummy counter that we send to Lab of Things Driver
  if (dummyValue < 1000)
    dummyValue++;
  else
   dummyValue = 0; 
   
  digitalWrite(led, HIGH);   // turn the LED on (HIGH is the voltage level)
  delay(1000);               // wait for a second
  digitalWrite(led, LOW);    // turn the LED off by making the voltage LOW
  delay(1000);               // wait for a second

}

int processCommandsFromLoT(int numCharRead ) {
  
   if (numCharRead < 2)  { //need at least '[' and one other character
       Serial.print("[!Badly formatted command]");  //! to indicate text string
       return -1; //skip the rest
     }
  
    //everything after '[' to the length read is the command.
    if (incomingData[0] != '[') {
       Serial.print("[!Badly formatted command]");  //! to indicate text string
       return -1; //skip the rest
    }
  
    //switch based on first character after the '['
      switch(incomingData[1]) {
        
        case '?':
         Serial.print("[HomeOSArduinoDevice_Dummy_MicrosoftResearch_1234]");
        break;
        
        ///ADD COMMANDS RELEVANT TO YOUR DEVICE & DRIVER HERE
        //example for dummy
        case 'v':
         Serial.print('[');
         Serial.print(dummyValue);
         Serial.print(']');     
         break;
        
        default:
          Serial.print("[!No matching command]");
      }

    
}

