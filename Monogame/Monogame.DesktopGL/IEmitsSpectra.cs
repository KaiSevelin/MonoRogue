namespace RoguelikeMonoGame
{
    // Any object that can be *emitted/sensed* across spectra (characters, items) carries this.
    public interface IEmitsSpectra
    {
        // How strongly this entity emits each spectrum.
        SpectrumVector Emission { get; }
    }

}
