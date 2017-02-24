# Unity-RIBBIT
I can't play video games without my http://frog.tips/

![frog tips in unity](http://i.imgur.com/bA6ljc5.png)

## Usage
Drop a component that utilizes RIBBITClient.cs or try the RIBBITClientExample.cs component.

## RIBBITClient API
### string FrogTip()
Gets a random frog tip, will perform a croak and return an empty string if none exist.

### IEnumerator Croak( bool refreshCache = false )
Will perform a web request to obtain more frog tips and add them to the current client memory.
Must be started with `StartCoroutine`, therefore results are not instant.
Setting refreshCache to true will clear all existing tips before downloading new ones.
