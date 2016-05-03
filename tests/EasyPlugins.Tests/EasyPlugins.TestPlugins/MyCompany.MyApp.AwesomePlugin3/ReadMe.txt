This plugin has an IEasyPlugin, with a manifest that has no Type and/or Assembly reference.

Because IEasyPlugin is the default type in the test to look for, it should load fine.

We are adding a few dummy package references to make sure that the plugin is found amidst multiple assembly references included in the plugin.