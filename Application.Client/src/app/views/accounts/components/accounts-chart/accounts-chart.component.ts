import { FlatTreeControl } from "@angular/cdk/tree";
import { Component } from "@angular/core";
import {
  MatTreeFlatDataSource,
  MatTreeFlattener,
} from "@angular/material/tree";
import { ChartOfAccount } from "../../models/chart-of-account.model";
import { AccountReportService } from "../../services/account-report.service";
/** Flat node with expandable and level information */
interface ExampleFlatNode {
  expandable: boolean;
  code: string;
  name: string;
  level: number;
}

@Component({
  selector: "app-accounts-chart",
  templateUrl: "./accounts-chart.component.html",
  styleUrls: ["./accounts-chart.component.scss"],
})
export class AccountsChartComponent {
  TREE_DATA: ChartOfAccount[];
  private _transformer = (node: ChartOfAccount, level: number) => {
    return {
      expandable: !!node.subAccountHeads && node.subAccountHeads.length > 0,
      code: node.code,
      name: node.name,
      level: level,
    };
  };

  treeControl = new FlatTreeControl<ExampleFlatNode>(
    (node) => node.level,
    (node) => node.expandable
  );

  treeFlattener = new MatTreeFlattener(
    this._transformer,
    (node) => node.level,
    (node) => node.expandable,
    (node) => node.subAccountHeads
  );

  dataSource = new MatTreeFlatDataSource(this.treeControl, this.treeFlattener);

  constructor(private accountReportService: AccountReportService) {
    this.getChartOfAccounts();
  }
  getChartOfAccounts(): void {
    this.accountReportService.getChartOfAccounts().subscribe((res) => {
      this.dataSource.data = res;
      this.TREE_DATA = res;
    });
  }
  hasChild = (_: number, node: ExampleFlatNode) => node.expandable;

  // filter recursively on a text string using property object value
  filterRecursive(filterText: string, array: any[], property: string) {
    let filteredData;

    //make a copy of the data so we don't mutate the original
    function copy(o: any) {
      return Object.assign({}, o);
    }

    // has string
    if (filterText) {
      // need the string to match the property value
      filterText = filterText.toLowerCase();
      // copy obj so we don't mutate it and filter
      filteredData = array.map(copy).filter(function x(y) {
        if (y[property].toLowerCase().includes(filterText)) {
          return true;
        }
        // if children match
        if (y.subAccountHeads) {
          return (y.subAccountHeads = y.subAccountHeads.map(copy).filter(x))
            .length;
        }
      });
      // no string, return whole array
    } else {
      filteredData = array;
    }

    return filteredData;
  }

  // pass mat input string to recursive function and return data
  filterTree(filterText: string) {
    // use filter input text, return filtered TREE_DATA, use the 'name' object value
    this.dataSource.data = this.filterRecursive(
      filterText,
      this.TREE_DATA,
      "name"
    );
  }

  // filter string from mat input filter
  applyFilter(filterText: string) {
    this.filterTree(filterText);
    // show / hide based on state of filter string
    if (filterText) {
      this.treeControl.expandAll();
    } else {
      this.treeControl.collapseAll();
    }
  }
}
