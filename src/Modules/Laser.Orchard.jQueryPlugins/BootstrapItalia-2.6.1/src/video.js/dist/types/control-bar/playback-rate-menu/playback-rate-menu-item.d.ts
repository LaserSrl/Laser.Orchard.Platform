export default PlaybackRateMenuItem;
/**
 * The specific menu item type for selecting a playback rate.
 *
 * @extends MenuItem
 */
declare class PlaybackRateMenuItem extends MenuItem {
    label: any;
    rate: number;
    /**
     * Update the PlaybackRateMenuItem when the playbackrate changes.
     *
     * @param {EventTarget~Event} [event]
     *        The `ratechange` event that caused this function to run.
     *
     * @listens Player#ratechange
     */
    update(event: any): void;
    /**
     * The text that should display over the `PlaybackRateMenuItem`s controls. Added for localization.
     *
     * @type {string}
     * @private
     */
    private contentElType;
}
import MenuItem from "../../menu/menu-item.js";
//# sourceMappingURL=playback-rate-menu-item.d.ts.map