export default MediaLoader;
/**
 * The `MediaLoader` is the `Component` that decides which playback technology to load
 * when a player is initialized.
 *
 * @extends Component
 */
declare class MediaLoader extends Component {
    /**
     * Create an instance of this class.
     *
     * @param {Player} player
     *        The `Player` that this class should attach to.
     *
     * @param {Object} [options]
     *        The key/value store of player options.
     *
     * @param {Component~ReadyCallback} [ready]
     *        The function that is run when this component is ready.
     */
    constructor(player: Player, options?: any, ready: any);
}
import Component from "../component.js";
//# sourceMappingURL=loader.d.ts.map