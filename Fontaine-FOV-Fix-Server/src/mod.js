"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const ScopeTemplates = require("../db/ScopeTemplates.json");
class Mod {
    postDBLoad(container) {
        const logger = container.resolve("WinstonLogger");
        const tables = container.resolve("DatabaseServer").getTables();
        const itemDB = tables.templates.items;
        for (let i in itemDB) {
            let serverItem = itemDB[i];
            if (serverItem._id === "5c07dd120db834001c39092d" || serverItem._id === "5c0a2cec0db834001b7ce47d" || serverItem._parent === "55818add4bdc2d5b648b456f" || serverItem._parent === "55818ae44bdc2dde698b456c" || serverItem._parent === "55818ae44bdc2dde698b456c" || serverItem._parent === "55818aeb4bdc2ddc698b456a") {
                for (let j in ScopeTemplates) {
                    let fileItem = ScopeTemplates[j];
                    if (fileItem.ItemID === serverItem._id) {
                        if (fileItem.IsFixed == true) {
                            serverItem._props.Zooms = [[fileItem.MinZoom]];
                        }
                        else {
                            serverItem._props.Zooms = [[fileItem.MinZoom, fileItem.MaxZoom]];
                        }
                        serverItem._props.HasShoulderContact = fileItem.IsFixed;
                        serverItem._props.ToolModdable = fileItem.CanToggle;
                    }
                }
            }
        }
    }
}
module.exports = { mod: new Mod() };
