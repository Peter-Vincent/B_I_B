# Bonsai In the Browser

Project to get Bonsai running in the browser, client side, using BlazorWASM.
* Run Bonsai workflows
* Run the Bonsai editor

## Objectives
* ~~Run simple Bonsai workflows in the browser~~
* ~~Read Bonsai files into the browser and use them to construct and run Bonsai workflows~~
* Allow access to hardware for Bonsai
  * Microphone
  * Speakers
  * Keypresses
  * Mouse
  * Screen
* Program to build the blazor file for easy and fast deployment
* Get the editor working
  
  
## To work on
* Get an interface working to access the hardware.  
  * Couple of different ways of doing this, some links below
    * [~~Keyboard events without input tags~~](https://stackoverflow.com/questions/58920461/how-to-detect-key-press-without-using-an-input-tag-in-blazor)
    * [~~Blazor event handling~~](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/event-handling?view=aspnetcore-3.1)
    * [~~More keyboard events~~](https://www.syncfusion.com/faq/blazor/event-handling/how-to-capture-input-keyboard-events)
  * It should be possible to use RxC# to get key presses and so on... 

### For consideration
For running the hardware interface, do we 
* use custom bonsai nodes for BlazorWASM?  
* Catch Blazor errors in the client and stitch on stuff to allow the desktop nodes to access the hardware above?

## Next meeting
Friday 11th September, 11am
