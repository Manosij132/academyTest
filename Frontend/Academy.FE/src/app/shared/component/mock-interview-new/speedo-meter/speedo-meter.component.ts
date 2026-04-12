import {
  Component,
  OnInit,
  Input,
  OnChanges,
  SimpleChanges,
  ChangeDetectorRef,
} from "@angular/core";
import * as d3 from "d3";
import { environment } from "../../../../../environments/environment";

interface GaugeConfig {
  size: number;
  clipWidth: number;
  clipHeight: number;
  ringInset: number;
  ringWidth: number;
  pointerWidth: number;
  pointerTailLength: number;
  pointerHeadLengthPercent: number;
  minValue: number;
  maxValue: number;
  minAngle: number;
  maxAngle: number;
  transitionMs: number;
  majorTicks: number;
  labelInset: number;
  arcColorFn: (t: number) => string;
}

@Component({
  selector: "speedo-meter-app",
  standalone: true,
  templateUrl: "./speedo-meter.component.html",
  styleUrls: ["./speedo-meter.component.css"],
})
export class SpeedoMeterComponent implements OnInit, OnChanges {
  @Input() speedScore: number = 50; // 0 - 100

  private gaugeInstance: any;
  private zoneColors = ["#ff4d4d", "#ff9900",  "#99cc33", "#00cc44"];
  private scoreLabels = [0, 25, 50, 75, 100]; // Numeric labels for gauge
  private scoreRanges = environment.speedRanges;

  constructor(private cd: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.drawGauge();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes["speedScore"] && this.gaugeInstance) {
      this.updateGauge();
      this.cd.detectChanges(); // Update label color dynamically
    }
  }

  /** Description text based on current score range */
  get scoreLabel(): string {
    const range = this.scoreRanges.find(
      (r) => this.speedScore >= r.min && this.speedScore <= r.max
    );
    return range ? range.label : "N/A";
  }

  /** Get dynamic color based on current score range */
  get scoreLabelColor(): string {
    const index = this.scoreRanges.findIndex(
      (r) => this.speedScore >= r.min && this.speedScore <= r.max
    );
    return index >= 0 ? this.zoneColors[index] : "#000000";
  }

  /** Draw the gauge using D3 */
  private drawGauge(): void {
    const gauge = (container: string, configuration: Partial<GaugeConfig>) => {
      const config: GaugeConfig = {
        size: 300,
        clipWidth: 300,
        clipHeight: 300,
        ringInset: 20,
        ringWidth: 40,
        pointerWidth: 8,
        pointerTailLength: 5,
        pointerHeadLengthPercent: 0.9,
        minValue: 0,
        maxValue: 100,
        minAngle: -90,
        maxAngle: 90,
        transitionMs: 1000,
        majorTicks: this.scoreLabels.length,
        labelInset: 25,
        arcColorFn: (t) =>
          this.zoneColors[
            Math.min(
              Math.floor(t * this.zoneColors.length),
              this.zoneColors.length - 1
            )
          ],
        ...configuration,
      };

      const r = config.size / 2;
      const pointerHeadLength = Math.round(r * config.pointerHeadLengthPercent);
      const deg2rad = (deg: number) => (deg * Math.PI) / 180;
      const centerTranslation = () => `translate(${r}, ${r})`;
      let pointer: any;

      const render = (newValue: number) => {
        const svg = d3
          .select(container)
          .append("svg")
          .attr("class", "gauge")
          .attr("width", config.clipWidth)
          .attr("height", config.clipHeight);

        const centerTx = centerTranslation();

        // Draw colored arcs for 5 ranges
        const arc = d3
          .arc()
          .innerRadius(r - config.ringWidth - config.ringInset)
          .outerRadius(r - config.ringInset)
          .startAngle((d: any, i: number) =>
            deg2rad(
              config.minAngle +
                (i * (config.maxAngle - config.minAngle)) /
                  (config.majorTicks - 1)
            )
          )
          .endAngle((d: any, i: number) =>
            deg2rad(
              config.minAngle +
                ((i + 1) * (config.maxAngle - config.minAngle)) /
                  (config.majorTicks - 1)
            )
          );

        const arcs = svg
          .append("g")
          .attr("class", "arc")
          .attr("transform", centerTx);

        arcs
          .selectAll("path")
          .data(d3.range(config.majorTicks - 1))
          .enter()
          .append("path")
          .attr("fill", (_, i) => this.zoneColors[i])
          .attr("d", arc as any);

        const lineData = [
          [0, -pointerHeadLength],
          [config.pointerWidth / 2, 0],
          [0, config.pointerTailLength],
          [-(config.pointerWidth / 2), 0],
          [0, -pointerHeadLength],
        ];

        const pointerLine = d3.line<any>().curve(d3.curveLinear);

        const pg = svg
          .append("g")
          .data([lineData])
          .attr("class", "pointer")
          .attr("transform", centerTx);

        pointer = pg
          .append("path")
          .attr("d", pointerLine as any)
          .attr("fill", "#000000")
          .style("filter", "drop-shadow(1px 2px 2px rgba(0,0,0,0.4))")
          .attr("transform", `rotate(${config.minAngle})`);

        update(newValue);
        return { update };
      };

      const update = (newValue: number) => {
        const clamped = Math.max(
          config.minValue,
          Math.min(newValue, config.maxValue)
        );
        const ratio =
          (clamped - config.minValue) / (config.maxValue - config.minValue);
        const newAngle =
          config.minAngle + ratio * (config.maxAngle - config.minAngle);

        pointer
          .transition()
          .duration(config.transitionMs)
          .ease(d3.easeElastic)
          .attr("transform", `rotate(${newAngle})`);
      };

      return { render, update };
    };

    this.gaugeInstance = gauge("#power-gauge", {});
    this.gaugeInstance.render(this.speedScore);
  }

  private updateGauge(): void {
    this.gaugeInstance.update(this.speedScore);
  }
}
