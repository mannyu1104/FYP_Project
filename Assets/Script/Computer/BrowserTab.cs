using System;

[Serializable]
public class BrowserTab
{
    // Id for this tab. 
    public readonly string TabId;

    public IBrowserPage Page { get; private set; }

    public BrowserTab(IBrowserPage page)
    {
        TabId = Guid.NewGuid().ToString();
        Page = page;
    }

    public void SetPage(IBrowserPage page)
    {
        Page = page;
    }
}