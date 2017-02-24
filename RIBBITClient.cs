using UnityEngine;
using System.Collections.Generic;

public class RIBBITClient : MonoBehaviour {
    public string gateway = "http://frog.tips/api/1";
    public string endpoint = "tips";
    public Dictionary<string,string> headers= new Dictionary<string, string>(){
        {"Accept", "application/der-stream"}
    };

    private Dictionary<long, string> croakDict = new Dictionary<long, string>();
    private List<string> croakUnordered = new List<string>();

    void OnEnable() {
        StartCoroutine(Croak());
    }

    public string FrogTip() {
        if( croakDict.Count == 0 ) {
            StartCoroutine( Croak( true ) );
            return "";
        }
        return croakUnordered[Random.Range( 0, croakUnordered.Count )];
    }

    public System.Collections.IEnumerator Croak( bool refreshCache = false ) {
        WWW www = new WWW( gateway + "/" + endpoint, null, headers );
        while( !www.isDone ) {
            yield return www;
        }

        var croakResp = DecodeANS1Der( www.bytes, www.bytes.Length, 0 );

        if( croakResp == null ) {
            Debug.LogError( "Error during CROAK request!" );
        } else {
            var croak = DecodeCroak( croakResp );

            if( refreshCache ) {
                croakDict.Clear();
                croakUnordered.Clear();
            }

            foreach( KeyValuePair<long, string> frogTip in croak ) {
                if( !croakDict.ContainsKey( frogTip.Key ) ) {
                    croakDict.Add( frogTip.Key, frogTip.Value );
                    croakUnordered.Add( frogTip.Value );
                }
            }
        }
    }

    // Unity doesn't ship with Array.Copy or Array.Slice so this is here.
    private static byte[] Slice( byte[] array, long start, long length ) {
        byte[] dest = new byte[length];

        for( int i = 0; i < length; i++ ) {
            dest[i] = array[start + i];
        }

        return dest;
    }

    private Dictionary<long, string> DecodeCroak( List<KeyValuePair<System.Type, object>> croak ) {
        var tips = new Dictionary<long, string>();

        var seq1 = ( List < KeyValuePair < System.Type, object>> )System.Convert.ChangeType( croak[0].Value, croak[0].Key );

        foreach( var innerSeq in seq1 ) {
            var seq2 = (List<KeyValuePair<System.Type, object>>) System.Convert.ChangeType( innerSeq.Value, innerSeq.Key );

            tips.Add(
                (long) System.Convert.ChangeType( seq2[0].Value, seq2[0].Key ),
                (string) System.Convert.ChangeType( seq2[1].Value, seq2[1].Key )
            );
        }

        return tips;
    }

    public static List<KeyValuePair<System.Type, object>> DecodeANS1Der( byte[] str, long len, long pos ) {
        var seq = new List<KeyValuePair<System.Type, object>>();
        long sPos = 0;

        var itemType = 0;
        var itemSize = 0;
        while( sPos < len ) {
            KeyValuePair<System.Type, object> newSeq;
            itemType = str[sPos]; sPos++;
            itemSize = (int) str[sPos]; sPos++;

            // calculate the item size, consuming a variable number of bytes
            if( itemSize > 128 ) {
                itemSize -= 128;

                var itemSizeCalc = 0;

                var itemSizeNext = -1;

                for( int i = 0; i < itemSize; i++ ) {
                    itemSizeCalc *= 256;
                    itemSizeNext = str[sPos]; sPos++;
                    itemSizeCalc += itemSizeNext;
                }

                itemSize = itemSizeCalc;
            }

            switch( itemType ) {
                case 0x02: // 02 = INTEGER
                    // calculate the value of the integer, consuming a variable number of bytes
                    long intValCalc = 0;
                    var intValNext = -1;

                    for( int i = 0; i < itemSize; i++ ) {
                        intValCalc *= 256;
                        intValNext = str[sPos]; sPos++;
                        intValCalc += intValNext;
                    }

                    long theVal = intValCalc;

                    newSeq = new KeyValuePair<System.Type, object>(
                        typeof( long ),
                        (long) theVal
                    );

                    seq.Add( newSeq );
                    break;
                case 0x0C: // 12 = UTF8String
                    var theVal2 = Slice( str, sPos, itemSize );
                    var theStr = System.Text.Encoding.UTF8.GetString( theVal2, 0, itemSize );
                    sPos += itemSize;

                    newSeq = new KeyValuePair<System.Type, object>(
                        typeof( string ),
                        theStr
                    );
                    seq.Add( newSeq );
                    break;
                case 0x30: // 48 = SEQUENCE
                    var inBytes = Slice( str, sPos, itemSize );

                    var theVal3 = DecodeANS1Der( inBytes, itemSize, 0 );
                    sPos += itemSize;

                    newSeq = new KeyValuePair<System.Type, object>(
                        typeof( List<KeyValuePair<System.Type, object>> ),
                        theVal3
                    );

                    seq.Add( newSeq );
                    break;
                default:
                    Debug.LogWarning( "Unexpected Data Tag: 0x" + itemType.ToString( "X" ) + "\nDecoder may have become misaligned!" );
                    break;
            }
        }
        return seq;
    }
}