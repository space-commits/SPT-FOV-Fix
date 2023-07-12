import { DependencyContainer } from "tsyringe";
import { ILogger } from "@spt-aki/models/spt/utils/ILogger";
import { DatabaseServer } from "@spt-aki/servers/DatabaseServer";
import { IPostDBLoadMod } from "@spt-aki/models/external/IPostDBLoadMod";

const ScopeTemplates = require("../db/ScopeTemplates.json");


class Mod implements IPostDBLoadMod {

    public postDBLoad(container: DependencyContainer): void {

        const logger = container.resolve<ILogger>("WinstonLogger");
        const tables = container.resolve<DatabaseServer>("DatabaseServer").getTables();
        const itemDB = tables.templates.items;

        for (let i in itemDB) {
            let serverItem = itemDB[i];
            if(serverItem._id === "5c07dd120db834001c39092d" || serverItem._id === "5c0a2cec0db834001b7ce47d" || serverItem._parent === "55818add4bdc2d5b648b456f" || serverItem._parent === "55818ae44bdc2dde698b456c" || serverItem._parent === "55818ae44bdc2dde698b456c" || serverItem._parent ==="55818aeb4bdc2ddc698b456a"){
                for(let j in ScopeTemplates){
                    let fileItem = ScopeTemplates[j];
                    if(fileItem.ItemID === serverItem._id){

                        if(fileItem.IsFixed == true){
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

module.exports = { mod: new Mod() }
