/* SystemJS module definition */
declare var module: NodeModule;
interface NodeModule {
  id: string;
}

/* moment locale files are plain JS with no declarations; TS 6's bundler
   resolution requires an ambient module for side-effect imports of them */
declare module "moment/locale/*";
