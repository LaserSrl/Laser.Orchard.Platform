/// <reference path="jquery.d.ts"/>

interface ExpandoOptions {
    collapse: bool;
    remember: bool;
}

interface JQuery {
    expandoControl(): JQuery;
    expandoControl(controller: Function, options: ExpandoOptions): JQuery;
}