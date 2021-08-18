using ER_Recogniser.ServiceModel;
using ServiceStack;
using System;
using System.Collections.Generic;

/// <summary>
/// A class for analizing video content.
/// </summary>
public class VideoAnalizerContext : IDisposable
{
    /// <summary>
    /// The service client
    /// </summary>
    JsonServiceClient serviceClient;
    /// <summary>
    /// The API key
    /// </summary>
    string apiKey = "";
    /// <summary>
    /// The analizer identifier
    /// </summary>
    string analizerID = "";
    /// <summary>
    /// The result
    /// </summary>
    List<VideoAnalizerResult> result = new List<VideoAnalizerResult>();

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoAnalizerContext"/> class.
    /// </summary>
    /// <param name="Apikey">The apikey.</param>
    /// <param name="ServiceClient">The service client.</param>
    /// <param name="Recognizer">The recognizer.</param>
    /// <param name="ThreshHold">The thresh hold.</param>
    public VideoAnalizerContext(string Apikey,JsonServiceClient ServiceClient,int Recognizer = 0,float ThreshHold = -1f)
    {
        this.serviceClient = ServiceClient;
        apiKey = Apikey;
        CreateAnalizerResponse response = serviceClient.Send<CreateAnalizerResponse>(new CreateAnalizer
        {
            ApiKey = apiKey,
            ThresHold = ThreshHold,
            Recognizer = Recognizer,
        });

        if (response.Status == "OK")
        {
            analizerID = response.AnalizerID;
        }
           
    }

    /// <summary>
    /// Analizes the frame.
    /// </summary>
    /// <param name="FrameImage">The frame image.</param>
    /// <param name="MimeType">Type of the MIME.</param>
    /// <param name="NewMembers">The new members.</param>
    /// <returns></returns>
    public bool AnalizeFrame(byte[] FrameImage,string MimeType,List<VideoAnalizerResult> NewMembers)
    {
        AnalizeFrameResponse response = serviceClient.Send<AnalizeFrameResponse>(new AnalizeFrame { ApiKey = apiKey, AnalizerID = analizerID, ImageData = FrameImage, MimeType = MimeType });
        if (response.Status == "OK")
        {
            NewMembers.AddRange(response.Result);
            return true;
        }
        return false;
    }

    #region IDisposable Support
    /// <summary>
    /// The disposed value
    /// </summary>
    private bool disposedValue = false; // To detect redundant calls

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
                RemoveAnalizerResponse response = serviceClient.Send<RemoveAnalizerResponse>(new RemoveAnalizer { ApiKey = apiKey, AnalizerID = analizerID });
                if (response.Status == "OK")
                {
                    analizerID = "";
                }
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            disposedValue = true;
        }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    // ~VideoAnalizerContext() {
    //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
    //   Dispose(false);
    // }

    // This code added to correctly implement the disposable pattern.
    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        // GC.SuppressFinalize(this);
    }
    #endregion
}