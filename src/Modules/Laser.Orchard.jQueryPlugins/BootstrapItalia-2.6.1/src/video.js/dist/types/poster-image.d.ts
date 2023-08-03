export default PosterImage;
/**
 * A `ClickableComponent` that handles showing the poster image for the player.
 *
 * @extends ClickableComponent
 */
declare class PosterImage extends ClickableComponent {
    /**
     * Create an instance of this class.
     *
     * @param {Player} player
     *        The `Player` that this class should attach to.
     *
     * @param {Object} [options]
     *        The key/value store of player options.
     */
    constructor(player: Player, options?: any);
    update_: (e: any) => void;
    /**
     * Create the `PosterImage`s DOM element.
     *
     * @return {Element}
     *         The element that gets created.
     */
    createEl(): Element;
    /**
     * Get or set the `PosterImage`'s crossOrigin option.
     *
     * @param {string|null} [value]
     *        The value to set the crossOrigin to. If an argument is
     *        given, must be one of `'anonymous'` or `'use-credentials'`, or 'null'.
     *
     * @return {string|null}
     *         - The current crossOrigin value of the `Player` when getting.
     *         - undefined when setting
     */
    crossOrigin(value?: string | null): string | null;
    /**
     * An {@link EventTarget~EventListener} for {@link Player#posterchange} events.
     *
     * @listens Player#posterchange
     *
     * @param {EventTarget~Event} [event]
     *        The `Player#posterchange` event that triggered this function.
     */
    update(event: any): void;
    /**
     * Set the source of the `PosterImage` depending on the display method.
     *
     * @param {string} url
     *        The URL to the source for the `PosterImage`.
     */
    setSrc(url: string): void;
    /**
     * An {@link EventTarget~EventListener} for clicks on the `PosterImage`. See
     * {@link ClickableComponent#handleClick} for instances where this will be triggered.
     *
     * @listens tap
     * @listens click
     * @listens keydown
     *
     * @param {EventTarget~Event} event
     +        The `click`, `tap` or `keydown` event that caused this function to be called.
     */
    handleClick(event: any): void;
    /**
     * Get or set the `PosterImage`'s crossorigin option. For the HTML5 player, this
     * sets the `crossOrigin` property on the `<img>` tag to control the CORS
     * behavior.
     *
     * @param {string|null} [value]
     *        The value to set the `PosterImages`'s crossorigin to. If an argument is
     *        given, must be one of `anonymous` or `use-credentials`.
     *
     * @return {string|null|undefined}
     *         - The current crossorigin value of the `Player` when getting.
     *         - undefined when setting
     */
    crossorigin: (value?: string | null) => string | null;
}
import ClickableComponent from "./clickable-component.js";
//# sourceMappingURL=poster-image.d.ts.map