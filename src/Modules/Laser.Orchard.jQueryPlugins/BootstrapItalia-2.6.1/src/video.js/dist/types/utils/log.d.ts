export default log;
declare const log: {
    (...args: Mixed[]): void;
    createLogger(subname: any): any;
    levels: any;
    level(lvl?: string): string;
    history: {
        (): any[];
        filter(fname: string): any[];
        clear(): void;
        disable(): void;
        enable(): void;
    };
    error(...args: Mixed[]): any;
    warn(...args: Mixed[]): any;
    debug(...args: Mixed[]): any;
};
export const createLogger: (subname: any) => any;
//# sourceMappingURL=log.d.ts.map